using Syncify.Sync.Application.DTOs;

namespace Syncify.Sync.Application.Ports;

public interface ISyncedEventRepository
{
    Task<SyncedEvent?> GetByRuleAndSourceEventAsync(Guid syncRuleId, string sourceEventId, CancellationToken ct = default);
    Task CreateAsync(SyncedEvent mapping, CancellationToken ct = default);
    Task UpdateAsync(SyncedEvent mapping, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
    Task DeleteAllByRuleAsync(Guid syncRuleId, CancellationToken ct = default);
}