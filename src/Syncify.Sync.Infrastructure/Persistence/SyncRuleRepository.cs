using Microsoft.EntityFrameworkCore;
using Syncify.Shared;
using Syncify.Sync.Application.Ports;
using Syncify.Sync.Domain.Aggregates;
using Syncify.Sync.Domain.Enums;
using Syncify.Sync.Infrastructure.Persistence.Mappers;

namespace Syncify.Sync.Infrastructure.Persistence;

internal sealed class SyncRuleRepository : ISyncRuleRepository
{
    private readonly SyncDbContext _db;

    public SyncRuleRepository(SyncDbContext db)
    {
        _db = db;
    }

    public async Task<SyncRule?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await _db.SyncRules
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, ct);

        return entity?.ToDomain();
    }

    public async Task CreateAsync(SyncRule rule, CancellationToken ct = default)
    {
        var entity = rule.ToEntity();
        _db.SyncRules.Add(entity);
        await _db.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(SyncRule rule, CancellationToken ct = default)
    {
        var entity = rule.ToEntity();
        _db.SyncRules.Update(entity);
        await _db.SaveChangesAsync(ct);
    }

    public async Task<IReadOnlyList<SyncRule>> ListByUserAsync(UserId userId, CancellationToken ct = default)
    {
        var entities = await _db.SyncRules
            .AsNoTracking()
            .Where(x => x.UserId == userId.Value)
            .ToListAsync(ct);

        return entities.Select(e => e.ToDomain()).ToList();
    }

    public async Task<IReadOnlyList<SyncRule>> ListActiveAsync(CancellationToken ct = default)
    {
        var activeStatus = SyncRuleStatus.Active.ToString();
        var entities = await _db.SyncRules
            .AsNoTracking()
            .Where(x => x.Status == activeStatus)
            .ToListAsync(ct);

        return entities.Select(e => e.ToDomain()).ToList();
    }
}
