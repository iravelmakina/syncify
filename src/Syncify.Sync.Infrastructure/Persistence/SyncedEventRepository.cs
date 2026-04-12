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
        var entity = await _db.SyncedEvents
            .FirstOrDefaultAsync(x => x.Id == mapping.Id, ct);

        if (entity == null)
            throw new InvalidOperationException($"Synced event {mapping.Id} not found.");

        var updated = mapping.ToEntity();
        
        _db.Entry(entity).CurrentValues.SetValues(updated);
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

    public async Task<IReadOnlyList<SyncedEventMapping>> ListByRuleSinceAsync(
        Guid syncRuleId, DateTime fromUtc, CancellationToken ct = default)
    {
        var entities = await _db.SyncedEvents
            .AsNoTracking()
            .Where(x => x.SyncRuleId == syncRuleId && x.SourceStart >= fromUtc)
            .ToListAsync(ct);

        return entities.Select(e => e.ToDto()).ToList();
    }

    public async Task DeleteByRuleSinceAsync(Guid syncRuleId, DateTime fromUtc, CancellationToken ct = default)
    {
        await _db.SyncedEvents
            .Where(x => x.SyncRuleId == syncRuleId && x.SourceStart >= fromUtc)
            .ExecuteDeleteAsync(ct);
    }
}
