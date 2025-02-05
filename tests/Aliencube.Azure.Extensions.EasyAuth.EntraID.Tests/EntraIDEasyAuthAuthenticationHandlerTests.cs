using System.Text;
using System.Text.Json;
using System.Text.Encodings.Web;

using Aliencube.Azure.Extensions.EasyAuth.Tests;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

using Shouldly;

namespace Aliencube.Azure.Extensions.EasyAuth.EntraID.Tests;

public class EntraIDEasyAuthAuthenticationHandlerTests : EasyAuthAuthenticationHandlerTestBase
{
    [Fact]
    public async Task HandleAuthenticateAsync_WithValidHeaders_ReturnsSuccess()
    {
        // Arrange
        var sample = new
        {
            auth_typ = "aad",
            name_typ = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress",
            role_typ = "http://schemas.microsoft.com/ws/2008/06/identity/claims/role",
            claims = new[]
            {
                new { typ = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress", val = "johndoe@contoso.com" },
                new { typ = "name", val = "John Doe" },
                new { typ = "roles", val = "User" }
            }
        };
        var json = JsonSerializer.Serialize(sample);
        var base64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(json));

        var context = new DefaultHttpContext();
        // Set header for provider: must be "aad" (case insensitive).
        context.Request.Headers["X-MS-CLIENT-PRINCIPAL-IDP"] = "aad";
        // Set client principal header.
        context.Request.Headers["X-MS-CLIENT-PRINCIPAL"] = base64;

        var scheme = new AuthenticationScheme(EasyAuthAuthenticationScheme.Name, EasyAuthAuthenticationScheme.Name, typeof(TestEntraIDEasyAuthAuthenticationHandler));
        var loggerFactory = LoggerFactory.Create(builder => builder.AddDebug());
        var encoder = UrlEncoder.Default;
        var optionsMonitor = CreateOptionsMonitor();

        var handler = new TestEntraIDEasyAuthAuthenticationHandler(optionsMonitor, loggerFactory, encoder);
        await handler.InitializeAsync(scheme, context);

        // Act
        var result = await handler.InvokeHandleAuthenticateAsync();

        // Assert
        result.Succeeded.ShouldBeTrue();
        result.Ticket.ShouldNotBeNull();
        result.Ticket.AuthenticationScheme.ShouldBe("aad");
        context.User.Identity?.IsAuthenticated.ShouldBeTrue();
    }

    [Fact]
    public async Task HandleAuthenticateAsync_WithInvalidProvider_ReturnsNoResult()
    {
        // Arrange
        var context = new DefaultHttpContext();
        // Provide a provider value other than expected "aad".
        context.Request.Headers["X-MS-TOKEN-AAD-ID-TOKEN"] = "invalid";
        context.Request.Headers["X-MS-CLIENT-PRINCIPAL"] = "dummy";

        var scheme = new AuthenticationScheme(EasyAuthAuthenticationScheme.Name, EasyAuthAuthenticationScheme.Name, typeof(TestEntraIDEasyAuthAuthenticationHandler));
        var loggerFactory = LoggerFactory.Create(builder => builder.AddDebug());
        var encoder = UrlEncoder.Default;
        var optionsMonitor = CreateOptionsMonitor();

        var handler = new TestEntraIDEasyAuthAuthenticationHandler(optionsMonitor, loggerFactory, encoder);
        await handler.InitializeAsync(scheme, context);

        // Act
        var result = await handler.InvokeHandleAuthenticateAsync();

        // Assert
        result.None.ShouldBeTrue();
    }
}
