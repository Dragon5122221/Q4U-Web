using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
namespace Q4UPrototypeWeb.Services;
public class FirebaseBearerHandler(IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder, IdentityService identity)
    : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var header = Request.Headers.Authorization.ToString();
        if (!header.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase)) return AuthenticateResult.NoResult();
        try { return AuthenticateResult.Success(new AuthenticationTicket(await identity.Verify(header[7..]), Scheme.Name)); }
        catch { return AuthenticateResult.Fail("Invalid or expired Firebase token or inactive membership."); }
    }
}
