using Microsoft.Extensions.Options;

namespace Aliencube.Azure.Extensions.EasyAuth.Tests;

public class TestOptionsMonitor : IOptionsMonitor<EasyAuthAuthenticationOptions>
{
    private readonly EasyAuthAuthenticationOptions _options;

    public TestOptionsMonitor(IOptions<EasyAuthAuthenticationOptions> options) => _options = options.Value;

    public EasyAuthAuthenticationOptions CurrentValue => _options;

    public EasyAuthAuthenticationOptions Get(string? name) => _options;

    public IDisposable OnChange(Action<EasyAuthAuthenticationOptions, string> listener) => default!;
}
