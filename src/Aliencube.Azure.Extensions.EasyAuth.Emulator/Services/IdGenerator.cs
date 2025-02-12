namespace Aliencube.Azure.Extensions.EasyAuth.Emulator.Services;

public class IdGenerator
{
    public Guid UserId { get; } = Guid.NewGuid();

    public Guid ClientId { get; } = Guid.NewGuid();

    public Guid TenantId { get; } = Guid.NewGuid();
}
