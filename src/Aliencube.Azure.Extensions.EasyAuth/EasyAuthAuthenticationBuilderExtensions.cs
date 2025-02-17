﻿using Microsoft.AspNetCore.Authentication;

namespace Aliencube.Azure.Extensions.EasyAuth;

/// <summary>
/// This represents the extension entity for <see cref="AuthenticationBuilder"/>.
/// </summary>
public static class EasyAuthAuthenticationBuilderExtensions
{
    /// <summary>
    /// Adds Azure EasyAuth handler.
    /// </summary>
    /// <param name="builder"><see cref="AuthenticationBuilder"/> instance.</param>
    /// <param name="configure"><see cref="Action"/> delegate instance that takes the <see cref="EasyAuthAuthenticationOptions"/> instance.</param>
    /// <returns>Returns <see cref="AuthenticationBuilder"/> instance.</returns>
    public static AuthenticationBuilder AddAzureEasyAuthHandler<THandler>(this AuthenticationBuilder builder, Action<EasyAuthAuthenticationOptions>? configure = default)
        where THandler : AuthenticationHandler<EasyAuthAuthenticationOptions>
    {
        if (configure == default)
        {
            configure = o => { };
        }

        return builder.AddScheme<EasyAuthAuthenticationOptions, THandler>(
            EasyAuthAuthenticationScheme.Name,
            EasyAuthAuthenticationScheme.Name,
            configure);
    }
}
