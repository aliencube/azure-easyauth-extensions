﻿using System.Security.Claims;
using System.Text.Encodings.Web;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Aliencube.Azure.Extensions.EasyAuth.EntraID;

/// <summary>
/// This represents the handler entity for Azure EasyAuth.
/// </summary>
/// <param name="options"><see cref="IOptionsMonitor{TOptions}"/> instance that takes <see cref="EasyAuthAuthenticationOptions"/>.</param>
/// <param name="logger"><see cref="ILoggerFactory"/> instance.</param>
/// <param name="encoder"><see cref="UrlEncoder"/> instance.</param>
public class EntraIDEasyAuthAuthenticationHandler(IOptionsMonitor<EasyAuthAuthenticationOptions> options, ILoggerFactory logger, UrlEncoder encoder)
    : EasyAuthAuthenticationHandler(options, logger, encoder)
{
    private const string AccessTokenHeaderName = "X-MS-TOKEN-AAD-ID-TOKEN";
    private const string IdentityProviderName = "aad";

    /// <summary>
    /// Gets the EasyAuth provider name from the request header.
    /// </summary>
    /// <returns>Returns the EasyAuth provider name. Default value is <c>aad</c>.</returns>
    protected override string? GetEasyAuthProvider()
    {
        var authProvider = Context.Request.Headers[IdentityProviderHeaderName].FirstOrDefault();

        return authProvider;
    }

    /// <inheritdoc />
    protected override bool IsAuthProviderExpected(string? authProvider)
    {
        if (string.IsNullOrWhiteSpace(authProvider) == true)
        {
            return false;
        }

        var expected = authProvider.ToLowerInvariant() == IdentityProviderName;

        return expected;
    }

    /// <inheritdoc />
    protected override string? GetClientPrincipal()
    {
        var encoded = Context.Request.Headers[ClientPrincipalHeaderName].FirstOrDefault();

        return encoded;
    }

    /// <inheritdoc />
    protected override async Task<ClaimsPrincipal?> GetClaimsPrincipal(string? encoded)
    {
        if (string.IsNullOrWhiteSpace(encoded) == true)
        {
            return default;
        }

        var principal = await MsClientPrincipal.ParseClaimsPrincipal(encoded).ConfigureAwait(false);

        return principal;
    }
}
