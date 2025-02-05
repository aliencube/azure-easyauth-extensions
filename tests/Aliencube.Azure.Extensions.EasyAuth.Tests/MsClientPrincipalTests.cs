using System.Text;
using System.Text.Json;

using Shouldly;

namespace Aliencube.Azure.Extensions.EasyAuth.Tests;

public class MsClientPrincipalTests
{
    [Fact]
    public async Task ParseMsClientPrincipal_WithValidBase64Json_ShouldReturnPrincipal()
    {
        // Arrange
        var sample = new
        {
            auth_typ = "aad",
            name_typ = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name",
            role_typ = "http://schemas.microsoft.com/ws/2008/06/identity/claims/role",
            claims = new[]
            {
                    new { typ = "name", val = "John Doe" },
                    new { typ = "email", val = "john@example.com" }
                }
        };
        var json = JsonSerializer.Serialize(sample);
        var base64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(json));

        // Act
        var principal = await MsClientPrincipal.ParseMsClientPrincipal(base64);

        // Assert
        principal.ShouldNotBeNull();
        principal!.IdentityProvider.ShouldBe("aad");
        principal.NameClaimType.ShouldNotBeNull();
        principal.RoleClaimType.ShouldNotBeNull();
        principal.Claims.ShouldNotBeNull();
    }

    [Fact]
    public async Task ParseMsClientPrincipal_WithInvalidBase64Json_ShouldReturnNull()
    {
        // Arrange
        var invalidBase64 = "invalid_base64";

        // Act & Assert
        await Should.ThrowAsync<FormatException>(async () =>
            await MsClientPrincipal.ParseMsClientPrincipal(invalidBase64));
    }
}
