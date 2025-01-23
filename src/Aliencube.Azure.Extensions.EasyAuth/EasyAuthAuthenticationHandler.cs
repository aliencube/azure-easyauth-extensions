using System.Text.Encodings.Web;

using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Aliencube.Azure.Extensions.EasyAuth;

/// <summary>
/// This represents the handler entity for Azure EasyAuth.
/// </summary>
/// <param name="options"><see cref="IOptionsMonitor{TOptions}"/> instance that takes <see cref="EasyAuthAuthenticationOptions"/>.</param>
/// <param name="logger"><see cref="ILoggerFactory"/> instance.</param>
/// <param name="encoder"><see cref="UrlEncoder"/> instance.</param>
public class EasyAuthAuthenticationHandler(IOptionsMonitor<EasyAuthAuthenticationOptions> options, ILoggerFactory logger, UrlEncoder encoder)
    : AuthenticationHandler<EasyAuthAuthenticationOptions>(options, logger, encoder)
{
    /// <inheritdoc />
    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        try
        {
            var easyAuthProvider = this.GetEasyAuthProvider();
            var encoded = Context.Request.Headers["X-MS-CLIENT-PRINCIPAL"].FirstOrDefault();
            if (string.IsNullOrWhiteSpace(encoded) == true)
            {
                return AuthenticateResult.NoResult();
            }

            var principal = await MsClientPrincipal.ParseClaimsPrincipal(encoded!).ConfigureAwait(false);
            if (principal == null)
            {
                return AuthenticateResult.NoResult();
            }

            var ticket = new AuthenticationTicket(principal, easyAuthProvider);
            var success = AuthenticateResult.Success(ticket);

            this.Context.User = principal;

            return success;
        }
        catch (Exception ex)
        {
            return AuthenticateResult.Fail(ex);
        }
    }

    /// <summary>
    /// Gets the EasyAuth provider name from the request header.
    /// </summary>
    /// <returns>Returns the EasyAuth provider name. Default value is an empty string.</returns>
    protected virtual string GetEasyAuthProvider()
    {
        return Context.Request.Headers["X-MS-CLIENT-PRINCIPAL-IDP"].FirstOrDefault() ?? string.Empty;
    }
}
