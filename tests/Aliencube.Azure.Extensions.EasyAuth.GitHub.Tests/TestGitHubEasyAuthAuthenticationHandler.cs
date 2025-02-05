using System.Text.Encodings.Web;

using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Aliencube.Azure.Extensions.EasyAuth.GitHub.Tests;

public class TestGitHubEasyAuthAuthenticationHandler(IOptionsMonitor<EasyAuthAuthenticationOptions> options, ILoggerFactory logger, UrlEncoder encoder)
    : GitHubEasyAuthAuthenticationHandler(options, logger, encoder)
{
    /// <summary>
    /// Invokes the protected HandleAuthenticateAsync method.
    /// </summary>
    /// <returns>Returns <see cref="AuthenticateResult"/> instance.</returns>
    public async Task<AuthenticateResult> InvokeHandleAuthenticateAsync()
        => await base.HandleAuthenticateAsync();
}
