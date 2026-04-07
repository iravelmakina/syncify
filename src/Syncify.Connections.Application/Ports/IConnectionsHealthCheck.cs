namespace Syncify.Connections.Application.Ports;

public interface IConnectionsHealthCheck
{
    Task<bool> IsHealthyAsync(CancellationToken ct = default);
}
