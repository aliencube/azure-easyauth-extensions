using Microsoft.AspNetCore.Authentication;

namespace Aliencube.Azure.Extensions.EasyAuth;

/// <summary>
/// This represents the options entity for Azure EasyAuth.
/// </summary>
public class EasyAuthAuthenticationOptions : AuthenticationSchemeOptions
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EasyAuthAuthenticationOptions"/> class.
    /// </summary>
    public EasyAuthAuthenticationOptions()
    {
        Events = new object();
    }
}
