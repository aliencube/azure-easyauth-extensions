using System.Text.Json;

namespace Aliencube.Azure.Extensions.EasyAuth.Emulator.Services;

public interface IEasyAuthService
{
    JsonSerializerOptions JsonSerializerOptions { get; }
    Guid UserId { get; }

    IEnumerable<string> UserRoles { get; }

    Task<IEnumerable<MsClientPrincipalClaim>> GetUserClaims(string? authenticationType);
}

public class EasyAuthService(UserIdGenerator userId) : IEasyAuthService
{
    public JsonSerializerOptions JsonSerializerOptions { get; } = new() { WriteIndented = true };
    public Guid UserId { get; } = userId.Value;
    public IEnumerable<string> UserRoles { get; } = [ "User", "Admin" ];
    public async Task<IEnumerable<MsClientPrincipalClaim>> GetUserClaims(string? authenticationType)
    {
        var claims = new List<MsClientPrincipalClaim>()
        {
            new() { Type = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier", Value = this.UserId.ToString() },
            new() { Type = "http://schemas.microsoft.com/ws/2008/06/identity/claims/role", Value = "User" },
            new() { Type = "http://schemas.microsoft.com/ws/2008/06/identity/claims/role", Value = "Admin" },
        };

        return await Task.FromResult(claims).ConfigureAwait(false);
    }
}
