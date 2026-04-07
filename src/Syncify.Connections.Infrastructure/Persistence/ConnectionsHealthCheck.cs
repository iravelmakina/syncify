using Syncify.Connections.Application.Ports;

namespace Syncify.Connections.Infrastructure.Persistence;

internal sealed class ConnectionsHealthCheck : IConnectionsHealthCheck
{
    private readonly ConnectionsDbContext _db;

    public ConnectionsHealthCheck(ConnectionsDbContext db)
    {
        _db = db;
    }

    public async Task<bool> IsHealthyAsync(CancellationToken ct = default)
    {
        return await _db.Database.CanConnectAsync(ct);
    }
}
