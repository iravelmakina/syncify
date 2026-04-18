using Syncify.Sync.Application.Ports;

namespace Syncify.Sync.Infrastructure.Persistence;

internal sealed class UnitOfWork : IUnitOfWork
{
    private readonly SyncDbContext _db;

    public UnitOfWork(SyncDbContext db)
    {
        _db = db;
    }

    public Task SaveChangesAsync(CancellationToken ct = default)
    {
        return _db.SaveChangesAsync(ct);
    }
}
