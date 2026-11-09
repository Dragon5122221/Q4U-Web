using System.ComponentModel.DataAnnotations;
namespace Q4UPrototypeWeb.Models;

public static class Roles
{
    public static readonly string[] All = ["Customer", "FleetOwner", "FleetManager", "Administrator", "Developer"];
    public static bool Operations(string role) => role is "FleetOwner" or "FleetManager" or "Administrator";
}
public record Actor(string Uid, string TenantId, string Role, string Name, bool Demo);
public class WorkspaceState
{
    public int SchemaVersion { get; set; } = 1;
    public long Version { get; set; }
    public string Organisation { get; set; } = "My Q4U account";
    public List<VehicleRecord> Vehicles { get; set; } = [];
    public List<RequestRecord> Requests { get; set; } = [];
    public List<DocumentRecord> Documents { get; set; } = [];
    public List<MemberRecord> Members { get; set; } = [];
    public List<NoticeRecord> Notices { get; set; } = [];
    public List<AuditRecord> Audit { get; set; } = [];
    public Dictionary<string, ProfileRecord> Profiles { get; set; } = [];
}
public class VehicleRecord
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string OwnerUid { get; set; } = "";
    public string Registration { get; set; } = "";
    public string Description { get; set; } = "";
    public DateOnly Expiry { get; set; }
    public string Driver { get; set; } = "Unassigned";
    public string Branch { get; set; } = "Main";
    public bool Archived { get; set; }
    public int DaysRemaining => Expiry.DayNumber - DateOnly.FromDateTime(DateTime.UtcNow.AddHours(2)).DayNumber;
}
public class RequestRecord
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string Reference => "Q4U-" + Id[..8].ToUpperInvariant();
    public string VehicleId { get; set; } = "";
    public string OwnerUid { get; set; } = "";
    public string Service { get; set; } = "";
    public string Status { get; set; } = "Submitted";
    public string Notes { get; set; } = "";
    public string Fulfilment { get; set; } = "Collection";
    public decimal Amount { get; set; }
    public bool Paid { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}
public class DocumentRecord
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string VehicleId { get; set; } = "";
    public string OwnerUid { get; set; } = "";
    public string FileName { get; set; } = "";
    public string ContentType { get; set; } = "";
    public string StorageKey { get; set; } = "";
    public string Category { get; set; } = "";
    public string Status { get; set; } = "Pending review";
    public string ReviewNote { get; set; } = "";
    public long Bytes { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}
public class MemberRecord
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string Email { get; set; } = "";
    public string Role { get; set; } = "FleetManager";
    public string Status { get; set; } = "Invitation draft";
}
public record NoticeRecord(string Id, string Uid, string Message, DateTimeOffset CreatedAt, bool Read = false);
public record AuditRecord(string Id, string Uid, string Action, DateTimeOffset CreatedAt);
public class ProfileRecord
{
    public string Name { get; set; } = "";
    public string Phone { get; set; } = "";
    public string Address { get; set; } = "";
    public bool RenewalReminders { get; set; } = true;
}
// The same versioned command envelope is used by MVC forms and the future mobile API.
public class WorkspaceCommand
{
    [MaxLength(25)] public string[] VehicleIds { get; set; } = [];
    [Required, StringLength(40)] public string Action { get; set; } = "";
    [Range(0, long.MaxValue)] public long Version { get; set; }
    [StringLength(100)] public string Id { get; set; } = "";
    [StringLength(200)] public string Value { get; set; } = "";
    [StringLength(1000)] public string Details { get; set; } = "";
    [StringLength(120)] public string Extra { get; set; } = "";
    public DateOnly? Date { get; set; }
    [Range(0, 100000)] public decimal Amount { get; set; }
}
public class WorkspacePage
{
    public required Actor Actor { get; set; }
    public required WorkspaceState State { get; set; }
    public string Section { get; set; } = "Dashboard";
    public string Query { get; set; } = "";
    public string Filter { get; set; } = "";
    public ProfileRecord Profile => State.Profiles.GetValueOrDefault(Actor.Uid) ?? new() { Name = Actor.Name };
}
public class DomainException(string message, int status = 400) : Exception(message)
{
    public int Status { get; } = status;
}
