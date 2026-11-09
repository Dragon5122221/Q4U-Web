using System.Globalization;
using System.Text.RegularExpressions;
using Google.Cloud.Firestore;
using Q4UPrototypeWeb.Models;
namespace Q4UPrototypeWeb.Services;

// Cloud Firestore repository: one Firestore document per business record.
//
//   memberships/{uid}                                  trusted role + tenant (created with tools/provision)
//   tenants/{tenantId}                                 organisation, version, schemaVersion, updatedAt
//   tenants/{tenantId}/vehicles/{vehicleId}
//   tenants/{tenantId}/requests/{requestId}
//   tenants/{tenantId}/documents/{documentId}          metadata only; the file itself is in Cloud Storage
//   tenants/{tenantId}/members/{invitationId}          invitation drafts
//   tenants/{tenantId}/users/{uid}                     profile + per-user version
//   tenants/{tenantId}/users/{uid}/notices/{noticeId}  in-app notifications
//   tenants/{tenantId}/audit/{auditId}                 append-only history
//
// Only the server touches these paths. firebase/firestore.rules denies every client SDK read and write.
//
// Reads are scoped to the actor: a customer only loads their own records, a developer only their profile,
// and organisation roles load the organisation. This keeps reads (and Firestore costs) proportional to what
// the person can actually see.
//
// Concurrency: organisation roles are guarded by tenants/{tenantId}.version; customers and developers by
// tenants/{tenantId}/users/{uid}.version. Every commit increments the tenant version and the version of every
// user whose records it touched, inside one transaction. A stale page can therefore never overwrite newer data,
// while one customer's changes do not force a different customer to refresh.
// It implements the original IWorkspaceRepository interface unchanged, so WorkspaceService and the controllers
// need no edits. The interface only passes the tenant ID, so the signed-in person is taken from the current request.
public sealed class FirestoreCollectionRepository(FirestoreDb db, IHttpContextAccessor http, IdentityService identity) : IWorkspaceRepository
{
    public const int SchemaVersion = 2;
    private const int HistoryLimit = 200;
    private static readonly Regex SafeId = new("^[A-Za-z0-9_-]{1,128}$", RegexOptions.Compiled);

    private enum Scope { Organisation, OwnRecords, ProfileOnly }

    // What was loaded, keyed by document path relative to the tenant document. Save compares against it and
    // writes only documents that changed. The repository is registered per request, so this is never shared.
    private sealed record Baseline(string TenantId, Scope Scope, string Uid, DocumentReference VersionDoc, long Version,
        Dictionary<string, Dictionary<string, object>> Documents);
    private Baseline? baseline;

    private DocumentReference TenantDoc(string tenantId) => db.Collection("tenants").Document(tenantId);

    private static Scope ScopeFor(Actor actor) => actor.Role switch
    {
        "FleetOwner" or "FleetManager" or "Administrator" => Scope.Organisation,
        "Developer" => Scope.ProfileOnly,
        _ => Scope.OwnRecords
    };

    public Task<WorkspaceState> Read(string tenant)
    {
        var user = http.HttpContext?.User ?? throw new DomainException("Sign in to continue.", 401);
        var actor = identity.Current(user);
        if (actor.TenantId != tenant) throw new DomainException("This workspace is not available to your account.", 403);
        return ReadAs(actor);
    }

    private async Task<WorkspaceState> ReadAs(Actor actor)
    {
        var scope = ScopeFor(actor);
        var tenant = TenantDoc(actor.TenantId);
        var user = tenant.Collection("users").Document(actor.Uid);
        var versionDoc = scope == Scope.Organisation ? tenant : user;

        // The concurrency token is read first. Any commit after this point changes it, so Save rejects a state
        // that could contain records from before that commit.
        var versionSnap = await versionDoc.GetSnapshotAsync();
        var tenantSnap = scope == Scope.Organisation ? versionSnap : await tenant.GetSnapshotAsync();
        var userSnap = scope == Scope.Organisation ? await user.GetSnapshotAsync() : versionSnap;

        var state = new WorkspaceState
        {
            SchemaVersion = SchemaVersion,
            Version = Long(versionSnap, "version"),
            Organisation = Str(tenantSnap, "organisation", "My Q4U account")
        };
        if (userSnap.Exists && userSnap.ContainsField("name")) state.Profiles[actor.Uid] = ToProfile(userSnap);

        if (scope != Scope.ProfileOnly)
        {
            Query Records(string collection) => scope == Scope.OwnRecords
                ? tenant.Collection(collection).WhereEqualTo("ownerUid", actor.Uid)
                : tenant.Collection(collection);

            var vehicles = Records("vehicles").GetSnapshotAsync();
            var requests = Records("requests").GetSnapshotAsync();
            var documents = Records("documents").GetSnapshotAsync();
            var notices = user.Collection("notices").OrderByDescending("createdAt").Limit(HistoryLimit).GetSnapshotAsync();
            var members = (actor.Role is "FleetOwner" or "Administrator") ? tenant.Collection("members").GetSnapshotAsync() : null;
            var audit = actor.Role == "Administrator"
                ? tenant.Collection("audit").OrderByDescending("createdAt").Limit(HistoryLimit).GetSnapshotAsync()
                : null;

            var pending = new List<Task> { vehicles, requests, documents, notices };
            if (members != null) pending.Add(members);
            if (audit != null) pending.Add(audit);
            await Task.WhenAll(pending);

            state.Vehicles = (await vehicles).Documents.Select(ToVehicle).OrderBy(v => v.Registration, StringComparer.Ordinal).ToList();
            state.Requests = (await requests).Documents.Select(ToRequest).OrderBy(r => r.CreatedAt).ToList();
            state.Documents = (await documents).Documents.Select(ToDocument).OrderBy(d => d.CreatedAt).ToList();
            // Queried newest first so the limit keeps the latest; stored oldest first, as the views expect.
            state.Notices = (await notices).Documents.Select(d => ToNotice(d, actor.Uid)).Reverse().ToList();
            if (members != null) state.Members = (await members).Documents.Select(ToMember).OrderBy(m => m.Email, StringComparer.OrdinalIgnoreCase).ToList();
            if (audit != null) state.Audit = (await audit).Documents.Select(ToAudit).Reverse().ToList();
        }

        baseline = new Baseline(actor.TenantId, scope, actor.Uid, versionDoc, state.Version, ToDocuments(state, actor.TenantId));
        return state;
    }

    public async Task Save(string tenantId, WorkspaceState state, long expectedVersion)
    {
        var before = baseline is { } loaded && loaded.TenantId == tenantId && loaded.Version == expectedVersion
            ? loaded
            : throw new DomainException("This page is out of date. Refresh and try again.", 409);

        var after = ToDocuments(state, tenantId);
        var writes = after.Where(doc => !before.Documents.TryGetValue(doc.Key, out var old) || !Same(old, doc.Value)).ToList();
        // The workflow only ever removes invitation drafts. Notices and audit entries are append-only: the service
        // trims its in-memory copy, but the history stays in Firestore.
        var deletes = before.Documents.Where(doc => !after.ContainsKey(doc.Key) && doc.Key.StartsWith("members/", StringComparison.Ordinal)).ToList();

        // Everyone whose records change gets a new version, so their open pages refresh before they save again.
        var bump = new HashSet<string>(StringComparer.Ordinal);
        foreach (var (path, data) in writes.Concat(deletes))
            if (OwnerOf(path, data) is { } owner) bump.Add(owner);
        if (before.Scope != Scope.Organisation) bump.Add(before.Uid);

        var tenant = TenantDoc(tenantId);
        await db.RunTransactionAsync(async transaction =>
        {
            // Transactions read before they write. If the token moved since Read, someone else committed first.
            var current = await transaction.GetSnapshotAsync(before.VersionDoc);
            if (Long(current, "version") != expectedVersion)
                throw new DomainException("Someone else updated these records. Refresh and try again.", 409);

            var userUpdates = new Dictionary<string, Dictionary<string, object>>(StringComparer.Ordinal);
            foreach (var (path, data) in writes)
            {
                if (ProfileUid(path) is { } profileUid)
                {
                    userUpdates[profileUid] = new Dictionary<string, object>(data);
                    continue;
                }
                transaction.Set(Doc(tenant, path), new Dictionary<string, object>(data) { ["updatedAt"] = FieldValue.ServerTimestamp });
            }
            foreach (var (path, _) in deletes) transaction.Delete(Doc(tenant, path));

            foreach (var uid in bump)
            {
                if (!userUpdates.TryGetValue(uid, out var update)) userUpdates[uid] = update = new Dictionary<string, object>();
                update["version"] = FieldValue.Increment(1L);
            }
            foreach (var (uid, update) in userUpdates)
            {
                update["updatedAt"] = FieldValue.ServerTimestamp;
                // Merge so a profile save never removes the version field, and a version bump never removes the profile.
                transaction.Set(Doc(tenant, "users/" + uid), update, SetOptions.MergeAll);
            }
            transaction.Set(tenant, new Dictionary<string, object>
            {
                ["version"] = FieldValue.Increment(1L),
                ["schemaVersion"] = SchemaVersion,
                ["updatedAt"] = FieldValue.ServerTimestamp
            }, SetOptions.MergeAll);
        });

        state.Version = expectedVersion + 1;
        baseline = null;
    }

    // ---- Record <-> Firestore document mapping -------------------------------------------------------------
    // Field names here are the Firestore schema. Money is stored as integer cents, dates as yyyy-MM-dd strings
    // and instants as Firestore timestamps (microsecond precision).

    private static Dictionary<string, Dictionary<string, object>> ToDocuments(WorkspaceState s, string tenantId)
    {
        var docs = new Dictionary<string, Dictionary<string, object>>(StringComparer.Ordinal);
        foreach (var v in s.Vehicles)
            docs["vehicles/" + v.Id] = new()
            {
                ["tenantId"] = tenantId,
                ["ownerUid"] = v.OwnerUid,
                ["registration"] = v.Registration,
                ["description"] = v.Description,
                ["expiry"] = v.Expiry.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                ["driver"] = v.Driver,
                ["branch"] = v.Branch,
                ["archived"] = v.Archived
            };
        foreach (var r in s.Requests)
            docs["requests/" + r.Id] = new()
            {
                ["tenantId"] = tenantId,
                ["reference"] = r.Reference,
                ["vehicleId"] = r.VehicleId,
                ["ownerUid"] = r.OwnerUid,
                ["service"] = r.Service,
                ["status"] = r.Status,
                ["notes"] = r.Notes,
                ["fulfilment"] = r.Fulfilment,
                ["amountCents"] = Cents(r.Amount),
                ["paid"] = r.Paid,
                ["createdAt"] = Ts(r.CreatedAt)
            };
        foreach (var d in s.Documents)
            docs["documents/" + d.Id] = new()
            {
                ["tenantId"] = tenantId,
                ["vehicleId"] = d.VehicleId,
                ["ownerUid"] = d.OwnerUid,
                ["fileName"] = d.FileName,
                ["contentType"] = d.ContentType,
                ["storageKey"] = d.StorageKey,
                ["category"] = d.Category,
                ["status"] = d.Status,
                ["reviewNote"] = d.ReviewNote,
                ["bytes"] = d.Bytes,
                ["createdAt"] = Ts(d.CreatedAt)
            };
        foreach (var m in s.Members)
            docs["members/" + m.Id] = new()
            {
                ["tenantId"] = tenantId,
                ["email"] = m.Email,
                ["role"] = m.Role,
                ["status"] = m.Status
            };
        foreach (var (uid, p) in s.Profiles)
            docs["users/" + uid] = new()
            {
                ["name"] = p.Name,
                ["phone"] = p.Phone,
                ["address"] = p.Address,
                ["renewalReminders"] = p.RenewalReminders
            };
        foreach (var n in s.Notices)
            docs[$"users/{n.Uid}/notices/{n.Id}"] = new()
            {
                ["tenantId"] = tenantId,
                ["uid"] = n.Uid,
                ["message"] = n.Message,
                ["read"] = n.Read,
                ["createdAt"] = Ts(n.CreatedAt)
            };
        foreach (var a in s.Audit)
            docs["audit/" + a.Id] = new()
            {
                ["tenantId"] = tenantId,
                ["uid"] = a.Uid,
                ["action"] = a.Action,
                ["createdAt"] = Ts(a.CreatedAt)
            };
        return docs;
    }

    private static VehicleRecord ToVehicle(DocumentSnapshot d) => new()
    {
        Id = d.Id,
        OwnerUid = Str(d, "ownerUid"),
        Registration = Str(d, "registration"),
        Description = Str(d, "description"),
        Expiry = Date(d, "expiry"),
        Driver = Str(d, "driver", "Unassigned"),
        Branch = Str(d, "branch", "Main"),
        Archived = Bool(d, "archived")
    };

    private static RequestRecord ToRequest(DocumentSnapshot d) => new()
    {
        Id = d.Id,
        VehicleId = Str(d, "vehicleId"),
        OwnerUid = Str(d, "ownerUid"),
        Service = Str(d, "service"),
        Status = Str(d, "status", "Submitted"),
        Notes = Str(d, "notes"),
        Fulfilment = Str(d, "fulfilment", "Collection"),
        Amount = Long(d, "amountCents") / 100m,
        Paid = Bool(d, "paid"),
        CreatedAt = Time(d, "createdAt")
    };

    private static DocumentRecord ToDocument(DocumentSnapshot d) => new()
    {
        Id = d.Id,
        VehicleId = Str(d, "vehicleId"),
        OwnerUid = Str(d, "ownerUid"),
        FileName = Str(d, "fileName"),
        ContentType = Str(d, "contentType"),
        StorageKey = Str(d, "storageKey"),
        Category = Str(d, "category"),
        Status = Str(d, "status", "Pending review"),
        ReviewNote = Str(d, "reviewNote"),
        Bytes = Long(d, "bytes"),
        CreatedAt = Time(d, "createdAt")
    };

    private static MemberRecord ToMember(DocumentSnapshot d) => new()
    {
        Id = d.Id,
        Email = Str(d, "email"),
        Role = Str(d, "role", "FleetManager"),
        Status = Str(d, "status", "Invitation draft")
    };

    private static ProfileRecord ToProfile(DocumentSnapshot d) => new()
    {
        Name = Str(d, "name"),
        Phone = Str(d, "phone"),
        Address = Str(d, "address"),
        RenewalReminders = Bool(d, "renewalReminders", true)
    };

    private static NoticeRecord ToNotice(DocumentSnapshot d, string uid) =>
        new(d.Id, Str(d, "uid", uid), Str(d, "message"), Time(d, "createdAt"), Bool(d, "read"));

    private static AuditRecord ToAudit(DocumentSnapshot d) =>
        new(d.Id, Str(d, "uid"), Str(d, "action"), Time(d, "createdAt"));

    // ---- Helpers ------------------------------------------------------------------------------------------

    private static string Str(DocumentSnapshot d, string field, string fallback = "") =>
        d.Exists && d.TryGetValue<string>(field, out var value) && value != null ? value : fallback;

    private static long Long(DocumentSnapshot d, string field) =>
        d.Exists && d.TryGetValue<long>(field, out var value) ? value : 0;

    private static bool Bool(DocumentSnapshot d, string field, bool fallback = false) =>
        d.Exists && d.TryGetValue<bool>(field, out var value) ? value : fallback;

    private static DateOnly Date(DocumentSnapshot d, string field) =>
        DateOnly.TryParseExact(Str(d, field), "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var value) ? value : default;

    private static DateTimeOffset Time(DocumentSnapshot d, string field) =>
        d.Exists && d.TryGetValue<Timestamp>(field, out var value) ? value.ToDateTimeOffset() : DateTimeOffset.UnixEpoch;

    // Firestore keeps microseconds, so truncate first; otherwise a value would differ after a round trip.
    private static Timestamp Ts(DateTimeOffset value) =>
        Timestamp.FromDateTimeOffset(new DateTimeOffset(value.UtcTicks - value.UtcTicks % 10, TimeSpan.Zero));

    private static long Cents(decimal amount) =>
        decimal.ToInt64(decimal.Round(amount * 100m, 0, MidpointRounding.AwayFromZero));

    private static bool Same(Dictionary<string, object> a, Dictionary<string, object> b) =>
        a.Count == b.Count && a.All(field => b.TryGetValue(field.Key, out var other) && object.Equals(field.Value, other));

    // "users/{uid}" is a profile document; "users/{uid}/notices/{id}" is not.
    private static string? ProfileUid(string path)
    {
        var parts = path.Split('/');
        return parts.Length == 2 && parts[0] == "users" ? parts[1] : null;
    }

    private static string? OwnerOf(string path, Dictionary<string, object> data)
    {
        if (ProfileUid(path) is { } uid) return uid;
        var owned = path.StartsWith("vehicles/", StringComparison.Ordinal)
                    || path.StartsWith("requests/", StringComparison.Ordinal)
                    || path.StartsWith("documents/", StringComparison.Ordinal);
        if (owned && data.TryGetValue("ownerUid", out var owner) && owner is string ownerUid && ownerUid.Length > 0) return ownerUid;
        return null;
    }

    // Builds tenants/{tenantId}/a/b/c/d from "a/b/c/d". IDs are server-generated GUIDs or Firebase UIDs; anything
    // else is refused so a crafted value can never point a write at another path.
    private static DocumentReference Doc(DocumentReference tenant, string path)
    {
        var parts = path.Split('/');
        if (parts.Length % 2 != 0 || parts.Any(part => !SafeId.IsMatch(part)))
            throw new InvalidOperationException("Refusing to write an unexpected Firestore path.");
        var doc = tenant;
        for (var i = 0; i < parts.Length; i += 2) doc = doc.Collection(parts[i]).Document(parts[i + 1]);
        return doc;
    }
}
