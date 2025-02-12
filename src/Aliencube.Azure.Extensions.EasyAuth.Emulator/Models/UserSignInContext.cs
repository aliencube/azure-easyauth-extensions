namespace Aliencube.Azure.Extensions.EasyAuth.Emulator.Models;

public record UserSignInContext(string? IdentityProvider, string? TenantId, string? ClientId, string? UserId, string? Username, string? UserRoles, string? UserClaims);
