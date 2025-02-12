using System.Text;
using System.Text.Json;

using Aliencube.Azure.Extensions.EasyAuth.Emulator.Models;

using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;

namespace Aliencube.Azure.Extensions.EasyAuth.Emulator.Services;

public interface IEasyAuthService
{
    JsonSerializerOptions JsonSerializerOptions { get; }

    Guid UserId { get; }

    Guid ClientId { get; }

    Guid TenantId { get; }

    IEnumerable<string> UserRoles { get; }

    IdentityProviderType GetIdentityProvider(string? identityProvider);

    Task<IEnumerable<MsClientPrincipalClaim>> GetDefaultUserClaims(string? identityProvider);

    Task<bool> UserSignInAsync(ProtectedSessionStorage session, UserSignInContext signInContext);
}

public class EasyAuthService(IHttpContextAccessor accessor, IdGenerator generator) : IEasyAuthService
{
    private readonly HttpContext? _context = accessor.HttpContext;

    public JsonSerializerOptions JsonSerializerOptions { get; } = new() { WriteIndented = true };

    public Guid UserId { get; } = generator.UserId;

    public Guid ClientId { get; } = generator.ClientId;

    public Guid TenantId { get; } = generator.TenantId;

    public IEnumerable<string> UserRoles { get; } = [ "User", "Admin" ];

    public IdentityProviderType GetIdentityProvider(string? identityProvider)
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

    public async Task<IEnumerable<MsClientPrincipalClaim>> GetDefaultUserClaims(string? identityProvider)
    {
        List<MsClientPrincipalClaim>? claims;
        var provider = this.GetIdentityProvider(identityProvider);
        if (provider == IdentityProviderType.None)
        {
            claims = [];
            return await Task.FromResult(claims).ConfigureAwait(false);
        }

        claims = provider switch
        {
            IdentityProviderType.EntraID =>
            [
                new() { Type = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/objectidentifier", Value = this.UserId.ToString() },
                new() { Type = "http://schemas.microsoft.com/ws/2008/06/identity/claims/role", Value = "User" },
                new() { Type = "http://schemas.microsoft.com/ws/2008/06/identity/claims/role", Value = "Admin" },
            ],
            IdentityProviderType.GitHub => [],
            _ => [],
        };

        return await Task.FromResult(claims).ConfigureAwait(false);
    }

    public async Task<bool> UserSignInAsync(ProtectedSessionStorage session, UserSignInContext signInContext)
    {
        var provider = this.GetIdentityProvider(signInContext.IdentityProvider);
        if (provider == IdentityProviderType.None)
        {
            return await Task.FromResult(false).ConfigureAwait(false);
        }

        var clientPrincipal = provider switch
        {
            IdentityProviderType.EntraID => this.BuildEntraIDMsClientPrincipal(signInContext),
            IdentityProviderType.GitHub => this.BuildGitHubMsClientPrincipal(signInContext),
            _ => null,
        };

        var serialised = JsonSerializer.Serialize(clientPrincipal, JsonSerializerOptions);
        var encoded = Convert.ToBase64String(Encoding.UTF8.GetBytes(serialised));
        var principal = new Dictionary<string, string>()
        {
            { "X-MS-CLIENT-PRINCIPAL-NAME", signInContext.Username! },
            { "X-MS-CLIENT-PRINCIPAL-ID", signInContext.UserId!.ToString() },
            { "X-MS-CLIENT-PRINCIPAL-IDP", signInContext.IdentityProvider! },
            { "X-MS-CLIENT-PRINCIPAL", encoded },
        };
        await session.SetAsync("easyauth", principal).ConfigureAwait(false);

        return await Task.FromResult(true).ConfigureAwait(false);
    }

    private MsClientPrincipal BuildEntraIDMsClientPrincipal(UserSignInContext context)
    {
        var utcNow = DateTimeOffset.UtcNow;
        var defaultClaims = new List<MsClientPrincipalClaim>()
        {
            new() { Type = "aud", Value = context.ClientId },
            new() { Type = "iss", Value = $"https://login.microsoftonline.com/{context.TenantId}/v2.0" },
            new() { Type = "iat", Value = $"{utcNow.ToUnixTimeSeconds()}" },
            new() { Type = "nbf", Value = $"{utcNow.ToUnixTimeSeconds()}" },
            new() { Type = "nbf", Value = $"{utcNow.AddMinutes(90).ToUnixTimeSeconds()}" },
            new() { Type = "aio", Value = $"{Convert.ToBase64String(Guid.NewGuid().ToByteArray())}" },
            new() { Type = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress", Value = context.Username },
            new() { Type = "http://schemas.microsoft.com/identity/claims/identityprovider", Value = $"https://sts.windows.net/{context.TenantId}/" },
        };

        var userRoleClaims = (context.UserRoles?.Split('\n', StringSplitOptions.RemoveEmptyEntries) ?? []).Select(p => new MsClientPrincipalClaim() { Type = "roles", Value = p });
        var userClaimClaims = JsonSerializer.Deserialize<IEnumerable<MsClientPrincipalClaim>>(context.UserClaims ?? "[]", JsonSerializerOptions) ?? [];

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
            IdentityProvider = context.IdentityProvider,
            NameClaimType = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress",
            RoleClaimType = "http://schemas.microsoft.com/ws/2008/06/identity/claims/role",
            Claims = claims,
        };

        return principal;
    }

    private MsClientPrincipal BuildGitHubMsClientPrincipal(UserSignInContext context)
    {
        throw new NotImplementedException();
    }
}
