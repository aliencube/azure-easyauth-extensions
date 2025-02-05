using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;

using Shouldly;

namespace Aliencube.Azure.Extensions.EasyAuth.Tests;

public class EasyAuthAuthenticationBuilderExtensionsTests
{
    [Fact]
    public async Task AddAzureEasyAuthHandler_WithDefaultConfiguration_ShouldRegisterScheme()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        var builder = services.AddAuthentication();

        // Act
        builder.AddAzureEasyAuthHandler<TestAuthenticationHandler>();
        var provider = services.BuildServiceProvider();
        var schemeProvider = provider.GetRequiredService<IAuthenticationSchemeProvider>();
        var scheme = await schemeProvider.GetSchemeAsync(EasyAuthAuthenticationScheme.Name);

        // Assert
        scheme.ShouldNotBeNull();
        scheme.Name.ShouldBe(EasyAuthAuthenticationScheme.Name);
    }
}
