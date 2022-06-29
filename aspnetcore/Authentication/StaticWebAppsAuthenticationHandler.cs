using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace SwaBackend.Authentication;

public class StaticWebAppsAuthenticationHandler
        : AuthenticationHandler<StaticWebAppsAuthenticationSchemeOptions>
{
    public StaticWebAppsAuthenticationHandler(
        IOptionsMonitor<StaticWebAppsAuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        ISystemClock clock)
        : base(options, logger, encoder, clock)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var hasHeader = this.Request.Headers.TryGetValue("x-ms-client-principal", out var authHeader);
        if (!hasHeader)
        {
            return Task.FromResult(AuthenticateResult.Fail("No x-ms-client-principal header found."));
        }

        var data = authHeader[0];
        var decoded = Convert.FromBase64String(data);
        var json = Encoding.UTF8.GetString(decoded);
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
        var swaClientPrincipal = JsonSerializer.Deserialize<StaticWebAppsClientPrincipal>(json, options) ?? new StaticWebAppsClientPrincipal();

        swaClientPrincipal.UserRoles = swaClientPrincipal.UserRoles.Except(new string[] { "anonymous" }, StringComparer.CurrentCultureIgnoreCase);
        var identity = new ClaimsIdentity(swaClientPrincipal.IdentityProvider);

        if (swaClientPrincipal.UserRoles.Any())
        {
            identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, swaClientPrincipal.UserId));
            identity.AddClaim(new Claim(ClaimTypes.Name, swaClientPrincipal.UserDetails));
            identity.AddClaims(swaClientPrincipal.UserRoles.Select(r => new Claim(ClaimTypes.Role, r)));
        }

        var ticket = new AuthenticationTicket(
            new ClaimsPrincipal(identity), this.Scheme.Name);
        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}

public class StaticWebAppsAuthenticationSchemeOptions
    : AuthenticationSchemeOptions
{
}

public class StaticWebAppsClientPrincipal
{
    public string IdentityProvider { get; set; } = "";
    public string UserId { get; set; } = "";
    public string UserDetails { get; set; } = "";
    public IEnumerable<string> UserRoles { get; set; } = new string[0];
}