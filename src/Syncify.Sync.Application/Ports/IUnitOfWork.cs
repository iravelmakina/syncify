namespace Syncify.Sync.Application.Ports;

public interface IUnitOfWork
{
    Task SaveChangesAsync(CancellationToken ct = default);
}
