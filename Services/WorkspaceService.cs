using Q4UPrototypeWeb.Models;
namespace Q4UPrototypeWeb.Services;

public class WorkspaceService(IWorkspaceRepository repository)
{
    public static readonly string[] Services = ["Licence renewal", "Number plates", "Change of ownership", "Vehicle registration", "Traffic fine assistance", "Personalised plates", "Roadworthy assistance", "Fleet enquiry", "Support enquiry", "Privacy request"];
    public static readonly string[] Sections = ["Dashboard", "Vehicles", "Renewals", "Requests", "Documents", "Payments", "Fleet", "Team", "Administration", "Developer", "Notifications", "Settings", "Help"];
    public static bool CanSection(Actor actor, string section) => section switch
    {
        "Developer" => actor.Role is "Developer" or "Administrator",
        "Administration" => actor.Role == "Administrator",
        "Team" => actor.Role is "FleetOwner" or "Administrator",
        "Fleet" => Roles.Operations(actor.Role),
        "Payments" => actor.Role is "Customer" or "FleetOwner" or "Administrator",
        "Settings" or "Help" => true,
        _ => actor.Role != "Developer"
    };
    public async Task<WorkspaceState> Get(Actor a) => Project(await repository.Read(a.TenantId), a);
    public static WorkspaceState Project(WorkspaceState state, Actor a)
    {
        if (a.Role == "Developer") return new() { Version = state.Version, Organisation = state.Organisation, Profiles = state.Profiles.Where(p => p.Key == a.Uid).ToDictionary() };
        if (a.Role == "Customer")
        {
            state.Vehicles = state.Vehicles.Where(v => v.OwnerUid == a.Uid).ToList();
            state.Requests = state.Requests.Where(r => r.OwnerUid == a.Uid).ToList();
            state.Documents = state.Documents.Where(d => d.OwnerUid == a.Uid).ToList();
        }
        if (a.Role is not ("Administrator" or "FleetOwner")) state.Members.Clear();
        if (a.Role != "Administrator") state.Audit.Clear();
        state.Notices = state.Notices.Where(n => n.Uid == a.Uid).ToList();
        state.Profiles = state.Profiles.Where(p => p.Key == a.Uid).ToDictionary();
        if (a.Role == "FleetManager") foreach (var r in state.Requests) { r.Amount = 0; r.Paid = false; }
        return state;
    }
    public async Task Apply(Actor a, WorkspaceCommand c, DocumentRecord? upload = null)
    {
        c.Id ??= ""; c.Value ??= ""; c.Details ??= ""; c.Extra ??= ""; c.VehicleIds ??= [];
        var s = await repository.Read(a.TenantId);
        if (s.Version != c.Version) throw new DomainException("This page is out of date. Refresh before saving your change.", 409);
        if (a.Role == "Developer" && c.Action != "profile") Deny();
        string Required(string value, string name) => !string.IsNullOrWhiteSpace(value) ? value.Trim() : throw new DomainException($"Enter {name}.");
        VehicleRecord Vehicle(string id) => s.Vehicles.FirstOrDefault(v => v.Id == id && (Roles.Operations(a.Role) || v.OwnerUid == a.Uid)) ?? throw new DomainException("Vehicle not found.", 404);
        RequestRecord Request(string id) => s.Requests.FirstOrDefault(r => r.Id == id && (Roles.Operations(a.Role) || r.OwnerUid == a.Uid)) ?? throw new DomainException("Request not found.", 404);
        void Notify(string uid, string message) => s.Notices.Add(new(Guid.NewGuid().ToString("N"), uid, message, DateTimeOffset.UtcNow));
        switch (c.Action)
        {
            case "vehicle":
                var registration = Required(c.Value, "the registration").ToUpperInvariant();
                if (!System.Text.RegularExpressions.Regex.IsMatch(registration, @"^[A-Z0-9 -]{3,20}$")) throw new DomainException("Use 3–20 letters, numbers, spaces or hyphens for the registration.");
                if (s.Vehicles.Any(v => v.Id != c.Id && v.Registration == registration)) throw new DomainException("This registration is already in the workspace.");
                if (c.Date is null || c.Date < new DateOnly(2000, 1, 1) || c.Date > DateOnly.FromDateTime(DateTime.UtcNow).AddYears(10)) throw new DomainException("Enter a valid licence expiry date, no more than ten years ahead.");
                var vehicle = string.IsNullOrEmpty(c.Id) ? new VehicleRecord { OwnerUid = a.Uid } : Vehicle(c.Id);
                if (vehicle.Archived) throw new DomainException("Restore the vehicle before editing it.");
                vehicle.Registration = registration; vehicle.Description = Required(c.Details, "the make, model and year"); vehicle.Expiry = c.Date.Value;
                if (string.IsNullOrEmpty(c.Id)) { if (s.Vehicles.Count >= 100) throw new DomainException("The prototype supports 100 vehicles per workspace."); s.Vehicles.Add(vehicle); }
                break;
            case "archive":
                var archived = Vehicle(c.Id);
                if (!archived.Archived && s.Requests.Any(r => r.VehicleId == c.Id && r.Status is not ("Completed" or "Cancelled"))) throw new DomainException("Complete or cancel the vehicle's open requests before archiving it.");
                archived.Archived = !archived.Archived; break;
            case "assignment":
                if (!Roles.Operations(a.Role)) Deny();
                var assigned = Vehicle(c.Id); assigned.Driver = Required(c.Value, "a driver name"); assigned.Branch = Required(c.Extra, "a branch"); break;
            case "bulkRenewal":
                if (!Roles.Operations(a.Role)) Deny();
                var selected = c.VehicleIds.Distinct().ToList();
                if (selected.Count is < 1 or > 25) throw new DomainException("Select between one and 25 vehicles.");
                var bulkVehicles = selected.Select(Vehicle).ToList();
                if (bulkVehicles.Any(v => v.Archived)) throw new DomainException("Archived vehicles cannot be renewed.");
                if (s.Requests.Any(r => selected.Contains(r.VehicleId) && r.Service == "Licence renewal" && r.Status is not ("Completed" or "Cancelled"))) throw new DomainException("One of the selected vehicles already has an open renewal. Deselect it before submitting.");
                if (s.Requests.Count + selected.Count > 200) throw new DomainException("The prototype request limit would be exceeded.");
                foreach (var item in bulkVehicles) s.Requests.Add(new() { VehicleId=item.Id, OwnerUid=a.Uid, Service="Licence renewal", Notes="Fleet batch renewal", Fulfilment="Collection" });
                Notify(a.Uid, $"{selected.Count} fleet renewal requests submitted for review.");
                break;
            case "request":
                if (!Services.Contains(c.Value)) throw new DomainException("Choose a supported service.");
                var requestVehicle = Vehicle(c.Id);
                if (requestVehicle.Archived) throw new DomainException("Restore this vehicle before requesting a service.");
                if (s.Requests.Count >= 200) throw new DomainException("The prototype supports 200 requests per workspace.");
                if (c.Extra is not ("Collection" or "Delivery")) throw new DomainException("Choose collection or delivery.");
                if (c.Extra == "Delivery" && string.IsNullOrWhiteSpace(c.Details)) throw new DomainException("Include the delivery address in your instructions.");
                var request = new RequestRecord { VehicleId=c.Id, OwnerUid=a.Uid, Service=c.Value, Notes=c.Details.Trim(), Fulfilment=c.Extra };
                s.Requests.Add(request); Notify(a.Uid, $"{request.Reference}: {request.Service} submitted."); break;
            case "cancel":
                var cancel = Request(c.Id);
                if (cancel.Paid || cancel.Status is not ("Submitted" or "Quoted")) throw new DomainException("This request cannot be cancelled here. Contact Q4U for assistance.");
                cancel.Status = "Cancelled"; Notify(cancel.OwnerUid, $"{cancel.Reference} cancelled."); break;
            case "quote":
                if (a.Role != "Administrator") Deny();
                var quoted = Request(c.Id);
                if (quoted.Status != "Submitted" || c.Amount <= 0) throw new DomainException("Only submitted requests can receive a positive quote.");
                quoted.Amount = c.Amount; quoted.Status = "Quoted"; Notify(quoted.OwnerUid, $"{quoted.Reference}: quote ready in Payments."); break;
            case "payDemo":
                if (!a.Demo || a.Role is not ("Customer" or "FleetOwner")) Deny();
                var paid = Request(c.Id);
                if (paid.Status != "Quoted" || paid.Paid || paid.Amount <= 0) throw new DomainException("There is no unpaid quote on this request.");
                paid.Paid = true; paid.Status = "Processing"; Notify(paid.OwnerUid, $"{paid.Reference}: demo payment recorded; no money was charged."); break;
            case "progress":
                if (a.Role != "Administrator") Deny();
                var progress = Request(c.Id);
                if (!(progress.Status == "Processing" && c.Value == "Ready" || progress.Status == "Ready" && c.Value == "Completed")) throw new DomainException("Requests progress from Processing to Ready, then Completed.");
                progress.Status = c.Value; Notify(progress.OwnerUid, $"{progress.Reference}: {c.Value.ToLowerInvariant()}."); break;
            case "upload":
                var documentVehicle = Vehicle(c.Id);
                if (documentVehicle.Archived) throw new DomainException("Restore this vehicle before uploading documents.");
                if (upload == null) throw new DomainException("Choose a file.");
                if (s.Documents.Count >= 100) throw new DomainException("The prototype supports 100 documents per workspace.");
                if (c.Value is not ("Identity" or "Proof of address" or "Registration" or "Licence disc" or "Other")) throw new DomainException("Choose a document category.");
                upload.VehicleId = c.Id; upload.OwnerUid = a.Uid; upload.Category=c.Value;
                s.Documents.Add(upload); break;
            case "review":
                if (a.Role != "Administrator") Deny();
                var review = s.Documents.FirstOrDefault(d => d.Id == c.Id) ?? throw new DomainException("Document not found.", 404);
                if (c.Value is not ("Verified" or "Rejected")) throw new DomainException("Choose a review outcome.");
                if (c.Value == "Rejected" && string.IsNullOrWhiteSpace(c.Details)) throw new DomainException("Explain why this document needs replacing.");
                review.Status = c.Value; review.ReviewNote = c.Details; Notify(review.OwnerUid, $"Document {review.FileName}: {review.Status}. {review.ReviewNote}"); break;
            case "invite":
                if (a.Role is not ("FleetOwner" or "Administrator")) Deny();
                if (!new System.ComponentModel.DataAnnotations.EmailAddressAttribute().IsValid(c.Value) || c.Extra is not ("Customer" or "FleetManager")) throw new DomainException("Enter a valid email and select Customer or Fleet Manager.");
                if (s.Members.Any(m => m.Email.Equals(c.Value.Trim(), StringComparison.OrdinalIgnoreCase))) throw new DomainException("An invitation draft already exists for that email.");
                if(s.Members.Count >= 50) throw new DomainException("The prototype supports 50 invitation drafts.");
                s.Members.Add(new() { Email=c.Value.Trim(), Role=c.Extra }); break;
            case "removeInvite":
                if (a.Role is not ("FleetOwner" or "Administrator")) Deny();
                s.Members.RemoveAll(m => m.Id == c.Id); break;
            case "profile":
                s.Profiles[a.Uid] = new() { Name=Required(c.Value, "your name"), Phone=c.Extra.Trim(), Address=c.Details.Trim(), RenewalReminders=c.Id == "on" }; break;
            case "read":
                s.Notices = s.Notices.Select(n => n.Uid == a.Uid && (c.Id == "all" || n.Id == c.Id) ? n with { Read = true } : n).ToList(); break;
            default: throw new DomainException("Unknown workspace action.");
        }
        s.Audit.Add(new(Guid.NewGuid().ToString("N"), a.Uid, c.Action, DateTimeOffset.UtcNow));
        s.Audit = s.Audit.TakeLast(200).ToList(); s.Notices = s.Notices.TakeLast(200).ToList();
        await repository.Save(a.TenantId, s, c.Version);
    }
    private static void Deny() => throw new DomainException("Your profile does not have permission for this action.", 403);
}
