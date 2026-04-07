using Syncify.Sync.Application.DTOs;

namespace Syncify.Sync.Application.Ports;

public interface ICalendarSyncer
{
    Task<FetchChangesResult> FetchChangesAsync(string providerCalendarId, string accessToken, string? cursor, CancellationToken ct = default);
    Task<string> CreateBlockAsync(string providerCalendarId, string accessToken, string title, DateTime start, DateTime end, CancellationToken ct = default);
    Task UpdateBlockAsync(string providerCalendarId, string accessToken, string blockId, string title, DateTime start, DateTime end, CancellationToken ct = default);
    Task DeleteBlockAsync(string providerCalendarId, string accessToken, string blockId, CancellationToken ct = default);
}
