using System.Text;
using System.Text.Json;
using System.Text.Encodings.Web;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

using Shouldly;

namespace Aliencube.Azure.Extensions.EasyAuth.Tests;

public class EasyAuthAuthenticationHandlerTests : EasyAuthAuthenticationHandlerTestBase
{
    [Fact]
    public async Task HandleAuthenticateAsync_WithValidHeaders_ReturnsSuccess()
    {
        // Arrange
        var sample = new
        {
            auth_typ = "test",
            name_typ = "name",
            role_typ = "role",
            claims = new[]
            {
                new { typ = "name", val = "John Doe" },
                new { typ = "email", val = "johndoe@contoso.com" },
                new { typ = "role", val = "User" }
            }
        };
        var json = JsonSerializer.Serialize(sample);
        var base64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(json));

        var context = new DefaultHttpContext();
        // Set provider header as expected.
        context.Request.Headers["X-MS-CLIENT-PRINCIPAL-IDP"] = "test";
        // Set client principal header.
        context.Request.Headers["X-MS-CLIENT-PRINCIPAL"] = base64;

        var scheme = new AuthenticationScheme(EasyAuthAuthenticationScheme.Name, EasyAuthAuthenticationScheme.Name, typeof(TestEasyAuthAuthenticationHandler));
        var loggerFactory = LoggerFactory.Create(builder => builder.AddDebug());
        var encoder = UrlEncoder.Default;
        var optionsMonitor = CreateOptionsMonitor();

        var handler = new TestEasyAuthAuthenticationHandler(optionsMonitor, loggerFactory, encoder);
        await handler.InitializeAsync(scheme, context);

        // Act
        var result = await handler.InvokeHandleAuthenticateAsync();

        // Assert
        result.Succeeded.ShouldBeTrue();
        result.Ticket.ShouldNotBeNull();
        result.Ticket.AuthenticationScheme.ShouldBe("test");
        context.User.Identity?.IsAuthenticated.ShouldBeTrue();
    }

    [Fact]
    public async Task HandleAuthenticateAsync_WithInvalidProvider_ReturnsNoResult()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Request.Headers["X-MS-CLIENT-PRINCIPAL-IDP"] = "invalid";
        context.Request.Headers["X-MS-CLIENT-PRINCIPAL"] = "dummy";

        var scheme = new AuthenticationScheme(EasyAuthAuthenticationScheme.Name, EasyAuthAuthenticationScheme.Name, typeof(TestEasyAuthAuthenticationHandler));
        var loggerFactory = LoggerFactory.Create(builder => builder.AddDebug());
        var encoder = UrlEncoder.Default;
        var optionsMonitor = CreateOptionsMonitor();

        var handler = new TestEasyAuthAuthenticationHandler(optionsMonitor, loggerFactory, encoder);
        await handler.InitializeAsync(scheme, context);

        // Act
        var result = await handler.InvokeHandleAuthenticateAsync();

        // Assert
        result.None.ShouldBeTrue();
    }
}
