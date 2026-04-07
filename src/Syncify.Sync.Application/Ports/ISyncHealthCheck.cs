namespace Syncify.Sync.Application.Ports;

public interface ISyncHealthCheck
{
    Task<bool> IsHealthyAsync(CancellationToken ct = default);
}
