using Syncify.Shared;
using Syncify.Sync.Domain.Aggregates;

namespace Syncify.Sync.Application.Ports;

public interface ISyncRuleRepository
{
    Task<SyncRule?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task CreateAsync(SyncRule rule, CancellationToken ct = default);
    void Add(SyncRule rule);  // For use with UnitOfWork - does not save
    Task UpdateAsync(SyncRule rule, CancellationToken ct = default);
    Task<IReadOnlyList<SyncRule>> ListByUserAsync(
        UserId userId, CancellationToken ct = default);
    Task<IReadOnlyList<SyncRule>> ListActiveAsync(CancellationToken ct = default);
}
