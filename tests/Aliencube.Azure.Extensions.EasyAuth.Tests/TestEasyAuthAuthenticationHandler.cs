using System.Security.Claims;
using System.Text.Encodings.Web;

using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Aliencube.Azure.Extensions.EasyAuth.Tests;

public class TestEasyAuthAuthenticationHandler(IOptionsMonitor<EasyAuthAuthenticationOptions> options, ILoggerFactory logger, UrlEncoder encoder)
    : EasyAuthAuthenticationHandler(options, logger, encoder)
{
    protected override string? GetEasyAuthProvider()
    {
        return Context.Request.Headers["X-MS-CLIENT-PRINCIPAL-IDP"];
    }

    protected override bool IsAuthProviderExpected(string? authProvider)
    {
        return string.Equals(authProvider, "test", StringComparison.InvariantCultureIgnoreCase);
    }

    protected override string? GetClientPrincipal()
    {
        return Context.Request.Headers["X-MS-CLIENT-PRINCIPAL"];
    }

    protected override async Task<ClaimsPrincipal?> GetClaimsPrincipal(MsClientPrincipal? clientPrincipal)
    {
        if (clientPrincipal == default)
        {
            return default;
        }

        var identity = new ClaimsIdentity("test");
        identity.AddClaim(new Claim("name", "John Doe"));

        return await Task.FromResult(new ClaimsPrincipal(identity));
    }

    /// <summary>
    /// Invokes the protected HandleAuthenticateAsync method.
    /// </summary>
    /// <returns>Returns <see cref="AuthenticateResult"/> instance.</returns>
    public async Task<AuthenticateResult> InvokeHandleAuthenticateAsync()
        => await base.HandleAuthenticateAsync();
}
