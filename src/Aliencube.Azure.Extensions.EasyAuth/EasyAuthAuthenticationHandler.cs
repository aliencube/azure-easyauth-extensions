using System.Security.Claims;
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
            var encoded = this.GetClientPrincipal();
            if (string.IsNullOrWhiteSpace(encoded) == true)
            {
                return AuthenticateResult.NoResult();
            }

            var principal = await this.GetClaimsPrincipal(encoded).ConfigureAwait(false);
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

    /// <summary>
    /// Gets the client principal from the request header.
    /// </summary>
    /// <returns>Returns the client principal.</returns>
    protected virtual string? GetClientPrincipal()
    {
        return default;
    }

    /// <summary>
    /// Gets the <see cref="ClaimsPrincipal"/> instance from the given value.
    /// </summary>
    /// <param name="encoded">The encoded client principal value.</param>
    /// <returns>Returns <see cref="ClaimsPrincipal"/> instance.</returns>
    protected virtual async Task<ClaimsPrincipal?> GetClaimsPrincipal(string encoded)
    {
        return await Task.FromResult<ClaimsPrincipal?>(default).ConfigureAwait(false);
    }
}
