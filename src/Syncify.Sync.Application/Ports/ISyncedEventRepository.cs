using Syncify.Sync.Application.DTOs;

namespace Syncify.Sync.Application.Ports;

public interface ISyncedEventRepository
{
    Task<SyncedEventMapping?> GetByRuleAndSourceEventAsync(Guid syncRuleId, string sourceEventId, CancellationToken ct = default);
    Task CreateAsync(SyncedEventMapping mapping, CancellationToken ct = default);
    Task UpdateAsync(SyncedEventMapping mapping, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
    Task DeleteAllByRuleAsync(Guid syncRuleId, CancellationToken ct = default);
}