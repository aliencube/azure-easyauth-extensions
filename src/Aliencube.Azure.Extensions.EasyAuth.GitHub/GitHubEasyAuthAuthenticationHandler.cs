using System.Security.Claims;
using System.Text.Encodings.Web;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Aliencube.Azure.Extensions.EasyAuth.GitHub;

/// <summary>
/// This represents the handler entity for Azure EasyAuth with GitHub.
/// </summary>
/// <param name="options"><see cref="IOptionsMonitor{TOptions}"/> instance that takes <see cref="EasyAuthAuthenticationOptions"/>.</param>
/// <param name="logger"><see cref="ILoggerFactory"/> instance.</param>
/// <param name="encoder"><see cref="UrlEncoder"/> instance.</param>
public class GitHubEasyAuthAuthenticationHandler(IOptionsMonitor<EasyAuthAuthenticationOptions> options, ILoggerFactory logger, UrlEncoder encoder)
    : EasyAuthAuthenticationHandler(options, logger, encoder)
{
    private const string IdentityProviderName = "github";

    /// <summary>
    /// Gets the EasyAuth provider name from the request header.
    /// </summary>
    /// <returns>Returns the EasyAuth provider name. Default value is <c>aad</c>.</returns>
    protected override string? GetEasyAuthProvider()
    {
        var authProvider = Context.Request.Headers[IdentityProviderHeaderName].FirstOrDefault();

        return authProvider;
    }

    /// <inheritdoc />
    protected override bool IsAuthProviderExpected(string? authProvider)
    {
        if (string.IsNullOrWhiteSpace(authProvider) == true)
        {
            return false;
        }

        var expected = authProvider.ToLowerInvariant() == IdentityProviderName;

        return expected;
    }

    /// <inheritdoc />
    protected override string? GetClientPrincipal()
    {
        var encoded = Context.Request.Headers[ClientPrincipalHeaderName].FirstOrDefault();

        return encoded;
    }

    /// <inheritdoc />
    protected override async Task<ClaimsPrincipal?> GetClaimsPrincipal(MsClientPrincipal? clientPrincipal)
    {
        if (clientPrincipal == default || clientPrincipal.Claims?.Any() == false)
        {
            return default;
        }

        var claims = clientPrincipal.Claims!.Select(claim => new Claim(claim.Type!, claim.Value!));

        // remap "urn:github:type" claims from easy auth to the more standard ClaimTypes.Role: "http://schemas.microsoft.com/ws/2008/06/identity/claims/role"
        var easyAuthRoleClaims = claims.Where(claim => claim.Type == "urn:github:type");
        var claimsAndRoles = claims.Concat(easyAuthRoleClaims.Select(role => new Claim(clientPrincipal.RoleClaimType!, role.Value)));

        var identity = new ClaimsIdentity(claimsAndRoles, clientPrincipal.IdentityProvider, clientPrincipal.NameClaimType, clientPrincipal.RoleClaimType);
        var claimsPrincipal = new ClaimsPrincipal(identity);

        return await Task.FromResult(claimsPrincipal).ConfigureAwait(false);
    }
}
