using System.Text;
using System.Text.Json;

using Aliencube.Azure.Extensions.EasyAuth.Emulator.Models;

namespace Aliencube.Azure.Extensions.EasyAuth.Emulator.Services;

public interface IEasyAuthService
{
    JsonSerializerOptions JsonSerializerOptions { get; }

    Guid UserId { get; }

    IEnumerable<string> UserRoles { get; }

    Task<IEnumerable<MsClientPrincipalClaim>> GetUserClaims(string? identityProvider);

    Task<bool> UserSignInAsync(HttpContext? context, string? identityProvider, string? userId, string? username, string? userRoles, string? userClaims);
}

public class EasyAuthService(UserIdGenerator userId) : IEasyAuthService
{
    public JsonSerializerOptions JsonSerializerOptions { get; } = new() { WriteIndented = true };

    public Guid UserId { get; } = userId.Value;

    public IEnumerable<string> UserRoles { get; } = [ "User", "Admin" ];

    public async Task<IEnumerable<MsClientPrincipalClaim>> GetUserClaims(string? identityProvider)
    {
        var claims = new List<MsClientPrincipalClaim>()
        {
            new() { Type = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier", Value = this.UserId.ToString() },
            new() { Type = "http://schemas.microsoft.com/ws/2008/06/identity/claims/role", Value = "User" },
            new() { Type = "http://schemas.microsoft.com/ws/2008/06/identity/claims/role", Value = "Admin" },
        };

        return await Task.FromResult(claims).ConfigureAwait(false);
    }

    public async Task<bool> UserSignInAsync(HttpContext? context, string? identityProvider, string? userId, string? username, string? userRoles, string? userClaims)
    {
        var provider = ParseIdentityProvider(identityProvider);
        if (provider == IdentityProviderType.None)
        {
            return await Task.FromResult(false).ConfigureAwait(false);
        }

        var clientPrincipal = provider switch
        {
            IdentityProviderType.EntraID => this.BuildEntraIDMsClientPrincipal(identityProvider, userId, username, userRoles, userClaims),
            IdentityProviderType.GitHub => this.BuildGitHubMsClientPrincipal(identityProvider, userId, username, userRoles, userClaims),
            _ => null,
        };

        context!.Response.Headers.Append("X-MS-CLIENT-PRINCIPAL-NAME", username!);
        context!.Response.Headers.Append("X-MS-CLIENT-PRINCIPAL-ID", Guid.NewGuid().ToString());
        context!.Response.Headers.Append("X-MS-CLIENT-PRINCIPAL-IDP", identityProvider!);

        var serialised = JsonSerializer.Serialize(clientPrincipal, JsonSerializerOptions);
        var encoded = Convert.ToBase64String(Encoding.UTF8.GetBytes(serialised));
        context!.Response.Headers.Append("X-MS-CLIENT-PRINCIPAL", encoded);

        return await Task.FromResult(true).ConfigureAwait(false);
    }

    private MsClientPrincipal BuildEntraIDMsClientPrincipal(string? identityProvider, string? userId, string? username, string? userRoles, string? userClaims)
    {
        var utcNow = DateTimeOffset.UtcNow;
        var defaultClaims = new List<MsClientPrincipalClaim>()
        {
            new() { Type = "aud", Value = userId },
            new() { Type = "iss", Value = "https://login.microsoftonline.com/00000000-0000-0000-0000-000000000000/v2.0" },
            new() { Type = "iat", Value = $"{utcNow.ToUnixTimeSeconds()}" },
            new() { Type = "nbf", Value = $"{utcNow.ToUnixTimeSeconds()}" },
            new() { Type = "nbf", Value = $"{utcNow.AddMinutes(90).ToUnixTimeSeconds()}" },
            new() { Type = "aio", Value = $"{Convert.ToBase64String(Guid.NewGuid().ToByteArray())}" },
            new() { Type = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress", Value = username },
            new() { Type = "http://schemas.microsoft.com/identity/claims/identityprovider", Value = "https://sts.windows.net/00000000-0000-0000-0000-000000000000/" },
        };

        var userRoleClaims = (userRoles?.Split('\n', StringSplitOptions.RemoveEmptyEntries) ?? []).Select(p => new MsClientPrincipalClaim() { Type = "roles", Value = p });
        var userClaimClaims = JsonSerializer.Deserialize<IEnumerable<MsClientPrincipalClaim>>(userClaims ?? "[]", JsonSerializerOptions) ?? [];

        var claims = new List<MsClientPrincipalClaim>();
        claims.AddRange(defaultClaims);
        if (userRoleClaims.Any() == true)
        {
            claims.AddRange(userRoleClaims);
        }
        if (userClaimClaims.Any() == true)
        {
            var userClaimClaimsWithoutUsername = userClaimClaims.Where(p => p.Type!.Equals("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress", StringComparison.InvariantCultureIgnoreCase) != true);
            claims.AddRange(userClaimClaimsWithoutUsername);
        }
        var principal = new MsClientPrincipal()
        {
            IdentityProvider = identityProvider,
            NameClaimType = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress",
            RoleClaimType = "http://schemas.microsoft.com/ws/2008/06/identity/claims/role",
            Claims = claims,
        };

        return principal;
    }

    private MsClientPrincipal BuildGitHubMsClientPrincipal(string? identityProvider, string? userId, string? username, string? userRoles, string? userClaims)
    {
        throw new NotImplementedException();
    }

    private static IdentityProviderType ParseIdentityProvider(string? identityProvider)
    {
        if (string.IsNullOrWhiteSpace(identityProvider) == true)
        {
            return IdentityProviderType.None;
        }

        if (identityProvider.Equals("aad", StringComparison.InvariantCultureIgnoreCase))
        {
            return IdentityProviderType.EntraID;
        }

        return Enum.TryParse<IdentityProviderType>(identityProvider, true, out var result) ? result : IdentityProviderType.None;
    }
}
