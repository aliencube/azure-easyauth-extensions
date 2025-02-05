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
    protected const string IdentityProviderHeaderName = "X-MS-CLIENT-PRINCIPAL-IDP";
    protected const string ClientPrincipalHeaderName = "X-MS-CLIENT-PRINCIPAL";

    /// <inheritdoc />
    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        try
        {
            var easyAuthProvider = this.GetEasyAuthProvider();
            if (this.IsAuthProviderExpected(easyAuthProvider) == false)
            {
                return AuthenticateResult.NoResult();
            }

            var encoded = this.GetClientPrincipal();
            if (string.IsNullOrWhiteSpace(encoded) == true)
            {
                return AuthenticateResult.NoResult();
            }

            var principal = await this.GetClaimsPrincipal(encoded).ConfigureAwait(false);
            if (principal == default)
            {
                return AuthenticateResult.NoResult();
            }

            var ticket = new AuthenticationTicket(principal, easyAuthProvider!);
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
    protected virtual string? GetEasyAuthProvider()
    {
        return default;
    }

    /// <summary>
    /// Gets the value indicating whether the given EasyAuth provider is expected or not.
    /// </summary>
    /// <param name="authProvider">Easy auth provider name.</param>
    /// <returns>Returns <c>True</c>, if expected; otherwise returns <c>False</c>.</returns>
    protected virtual bool IsAuthProviderExpected(string? authProvider)
    {
        return false;
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
    /// <param name="clientPrincipal"><see cref="MsClientPrincipal"/> instance.</param>
    /// <returns>Returns <see cref="ClaimsPrincipal"/> instance.</returns>
    protected virtual async Task<ClaimsPrincipal?> GetClaimsPrincipal(MsClientPrincipal? clientPrincipal)
    {
        return await Task.FromResult<ClaimsPrincipal?>(default).ConfigureAwait(false);
    }

    private async Task<ClaimsPrincipal?> GetClaimsPrincipal(string? encoded)
    {
        if (string.IsNullOrWhiteSpace(encoded) == true)
        {
            return default;
        }

        var clientPrincipal = await MsClientPrincipal.ParseMsClientPrincipal(encoded!).ConfigureAwait(false);

        return await this.GetClaimsPrincipal(clientPrincipal).ConfigureAwait(false);
    }
}
