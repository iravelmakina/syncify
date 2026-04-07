using Syncify.Sync.Application.Ports;

namespace Syncify.Sync.Infrastructure.Persistence;

internal sealed class SyncHealthCheck : ISyncHealthCheck
{
    private readonly SyncDbContext _db;

    public SyncHealthCheck(SyncDbContext db)
    {
        _db = db;
    }

    public async Task<bool> IsHealthyAsync(CancellationToken ct = default)
    {
        return await _db.Database.CanConnectAsync(ct);
    }
}
