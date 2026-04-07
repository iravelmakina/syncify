using Microsoft.EntityFrameworkCore;
using Syncify.Sync.Application.DTOs;
using Syncify.Sync.Application.Ports;
using Syncify.Sync.Infrastructure.Persistence.Mappers;

namespace Syncify.Sync.Infrastructure.Persistence;

internal sealed class SyncedEventRepository : ISyncedEventRepository
{
    private readonly SyncDbContext _db;

    public SyncedEventRepository(SyncDbContext db)
    {
        _db = db;
    }

    public async Task<SyncedEventMapping?> GetByRuleAndSourceEventAsync(
        Guid syncRuleId,
        string sourceEventId,
        CancellationToken ct = default)
    {
        var entity = await _db.SyncedEvents
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.SyncRuleId == syncRuleId && x.SourceEventId == sourceEventId, ct);

        return entity?.ToDto();
    }

    public async Task CreateAsync(SyncedEventMapping mapping, CancellationToken ct = default)
    {
        var entity = mapping.ToEntity();
        _db.SyncedEvents.Add(entity);
        await _db.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(SyncedEventMapping mapping, CancellationToken ct = default)
    {
        var entity = mapping.ToEntity();
        _db.SyncedEvents.Update(entity);
        await _db.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await _db.SyncedEvents.FindAsync(new object[] { id }, ct);
        if (entity != null)
        {
            _db.SyncedEvents.Remove(entity);
            await _db.SaveChangesAsync(ct);
        }
    }

    public async Task DeleteAllByRuleAsync(Guid syncRuleId, CancellationToken ct = default)
    {
        await _db.SyncedEvents
            .Where(x => x.SyncRuleId == syncRuleId)
            .ExecuteDeleteAsync(ct);
    }
}
