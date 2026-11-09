using System.Security.Claims;
using FirebaseAdmin.Auth;
using Google.Cloud.Firestore;
using Q4UPrototypeWeb.Models;
namespace Q4UPrototypeWeb.Services;

public class IdentityService(IConfiguration configuration, IWebHostEnvironment environment, IServiceProvider services)
{
    public bool Demo => configuration["Q4U:Mode"] == "Demo" && environment.IsDevelopment();
    public Actor Current(ClaimsPrincipal user) => new(
        user.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new DomainException("Sign in to continue.", 401),
        user.FindFirstValue("tenant") ?? throw new DomainException("No workspace assigned.", 403),
        user.FindFirstValue(ClaimTypes.Role) ?? "", user.Identity?.Name ?? "Q4U user", Demo);
    public async Task<ClaimsPrincipal> Verify(string token)
    {
        if (Demo) throw new DomainException("Firebase sign-in is disabled in demo mode.");
        var decoded = await services.GetRequiredService<FirebaseAuth>().VerifyIdTokenAsync(token, true);
        // Membership is read server-side on every request; client inputs never assign a role or tenant.
        var doc = await services.GetRequiredService<FirestoreDb>().Collection("memberships").Document(decoded.Uid).GetSnapshotAsync();
        if (!doc.Exists || !doc.GetValue<bool>("active")) throw new DomainException("Your account needs an active Q4U workspace membership.", 403);
        var role = doc.GetValue<string>("role"); var tenant = doc.GetValue<string>("tenantId");
        if (!Roles.All.Contains(role) || !System.Text.RegularExpressions.Regex.IsMatch(tenant, @"^[a-zA-Z0-9_-]{1,80}$")) throw new DomainException("Invalid workspace membership.", 403);
        var name = decoded.Claims.GetValueOrDefault("name")?.ToString() ?? "Q4U user";
        return Principal(decoded.Uid, tenant, role, name, token);
    }
    public static ClaimsPrincipal Principal(string uid, string tenant, string role, string name, string? token = null)
    {
        var claims = new List<Claim> { new(ClaimTypes.NameIdentifier,uid), new("tenant",tenant), new(ClaimTypes.Role,role), new(ClaimTypes.Name,name) };
        if (token != null) claims.Add(new("firebase_token",token));
        return new ClaimsPrincipal(new ClaimsIdentity(claims, "Q4U"));
    }
}
