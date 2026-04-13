using Syncify.Sync.Application.Models;

namespace Syncify.Sync.Application.Ports;

public interface ISyncedEventRepository
{
    Task<SyncedEventMapping?> GetByRuleAndSourceEventAsync(Guid syncRuleId, string sourceEventId, CancellationToken ct = default);
    Task<IReadOnlyList<SyncedEventMapping>> ListByRuleSinceAsync(Guid syncRuleId, DateTime fromUtc, CancellationToken ct = default);
    Task CreateAsync(SyncedEventMapping mapping, CancellationToken ct = default);
    Task UpdateAsync(SyncedEventMapping mapping, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
    Task DeleteAllByRuleAsync(Guid syncRuleId, CancellationToken ct = default);
    Task DeleteByRuleSinceAsync(Guid syncRuleId, DateTime fromUtc, CancellationToken ct = default);
}