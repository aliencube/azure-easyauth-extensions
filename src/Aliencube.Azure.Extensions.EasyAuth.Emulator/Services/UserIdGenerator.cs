namespace Aliencube.Azure.Extensions.EasyAuth.Emulator.Services;

public class UserIdGenerator
{
    public Guid Value { get; } = Guid.NewGuid();
}
