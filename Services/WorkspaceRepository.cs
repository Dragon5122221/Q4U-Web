using System.Collections.Concurrent;
using System.Text.Json;
using Google.Cloud.Firestore;
using Microsoft.Extensions.Caching.Memory;
using Q4UPrototypeWeb.Models;
namespace Q4UPrototypeWeb.Services;

public interface IWorkspaceRepository
{
    Task<WorkspaceState> Read(string tenant);
    Task Save(string tenant, WorkspaceState state, long expectedVersion);
}
public static class StateJson
{
    public static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web);
    public static string Write(WorkspaceState state) => JsonSerializer.Serialize(state, Options);
    public static WorkspaceState Read(string json) => JsonSerializer.Deserialize<WorkspaceState>(json, Options)!;
}
// Demo data is isolated by an unguessable browser workspace, with a bounded cache lifetime.
public class DemoWorkspaceRepository(IMemoryCache cache) : IWorkspaceRepository
{
    private readonly object gate = new();
    public Task<WorkspaceState> Read(string tenant)
    {
        lock (gate)
        {
            if (!cache.TryGetValue<string>(tenant, out var json))
            {
                json = StateJson.Write(Seed());
                cache.Set(tenant, json, TimeSpan.FromHours(8));
            }
            return Task.FromResult(StateJson.Read(json!));
        }
    }
    public Task Save(string tenant, WorkspaceState state, long expectedVersion)
    {
        lock (gate)
        {
            if (!cache.TryGetValue<string>(tenant, out var json) || StateJson.Read(json!).Version != expectedVersion)
                throw new DomainException("This workspace changed or expired. Refresh the page and try again.", 409);
            state.Version = expectedVersion + 1;
            cache.Set(tenant, StateJson.Write(state), TimeSpan.FromHours(8));
        }
        return Task.CompletedTask;
    }
    private static WorkspaceState Seed()
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow.AddHours(2));
        var state = new WorkspaceState { Organisation = "Q4U demonstration fleet" };
        state.Vehicles.AddRange([
            new() { Id="polo", OwnerUid="demo-customer", Registration="JT 42 ZN GP", Description="Volkswagen Polo · 2021", Expiry=today.AddDays(25), Driver="Ayesha Khan", Branch="Sandton" },
            new() { Id="hilux", OwnerUid="demo-fleetowner", Registration="LM 81 TK GP", Description="Toyota Hilux · 2022", Expiry=today.AddDays(145), Driver="Thabo Molefe", Branch="Johannesburg" },
            new() { Id="bmw", OwnerUid="demo-customer", Registration="KP 19 FX GP", Description="BMW 320i · 2020", Expiry=today.AddDays(6), Driver="Ayesha Khan", Branch="Sandton" }
        ]);
        state.Requests.Add(new() { Id="a18b6024cabc412781de84807d02aa11", VehicleId="bmw", OwnerUid="demo-customer", Service="Licence renewal", Status="Quoted", Amount=1284.50m, Notes="Sample quote for demonstration only." });
        state.Notices.Add(new(Guid.NewGuid().ToString("N"), "demo-customer", "Your BMW renewal quote is ready. Open Payments to review it.", DateTimeOffset.UtcNow));
        return state;
    }
}
// Prototype adapter: bounded tenant aggregate, atomic version check, no direct client writes.
public class FirestoreWorkspaceRepository(FirestoreDb db) : IWorkspaceRepository
{
    private DocumentReference Ref(string tenant) => db.Collection("tenants").Document(tenant).Collection("workspace").Document("state");
    public async Task<WorkspaceState> Read(string tenant)
    {
        var doc = await Ref(tenant).GetSnapshotAsync();
        return doc.Exists ? StateJson.Read(doc.GetValue<string>("json")) : new WorkspaceState();
    }
    public async Task Save(string tenant, WorkspaceState state, long expectedVersion)
    {
        await db.RunTransactionAsync(async transaction =>
        {
            var reference = Ref(tenant);
            var doc = await transaction.GetSnapshotAsync(reference);
            var version = doc.Exists ? doc.GetValue<long>("version") : 0;
            if (version != expectedVersion) throw new DomainException("Someone updated this workspace. Refresh and try again.", 409);
            state.Version = version + 1;
            var json = StateJson.Write(state);
            if (System.Text.Encoding.UTF8.GetByteCount(json) > 700000)
                throw new DomainException("Prototype workspace capacity reached. Export records and contact your administrator.", 409);
            transaction.Set(reference, new Dictionary<string, object> { ["json"] = json, ["version"] = state.Version, ["schemaVersion"] = 1, ["updatedAt"] = FieldValue.ServerTimestamp });
        });
    }
}
