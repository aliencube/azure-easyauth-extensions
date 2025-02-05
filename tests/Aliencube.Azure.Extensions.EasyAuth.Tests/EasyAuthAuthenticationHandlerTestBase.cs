using Microsoft.Extensions.Options;

namespace Aliencube.Azure.Extensions.EasyAuth.Tests;

public class EasyAuthAuthenticationHandlerTestBase
{
    protected IOptionsMonitor<EasyAuthAuthenticationOptions> CreateOptionsMonitor()
    {
        var options = Options.Create(new EasyAuthAuthenticationOptions());

        return new TestOptionsMonitor(options);
    }
}
