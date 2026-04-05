using Syncify.Sync.Application.DTOs;

namespace Syncify.Sync.Application.Ports;

public interface ICalendarSyncer
{
    Task<FetchChangesResult> FetchChangesAsync(Guid calendarId, string accessToken, string? cursor, CancellationToken ct = default);
    Task<string> CreateBlockAsync(Guid calendarId, string accessToken, string title, DateTime start, DateTime end, CancellationToken ct = default);
    Task UpdateBlockAsync(Guid calendarId, string accessToken, string blockId, string title, DateTime start, DateTime end, CancellationToken ct = default);
    Task DeleteBlockAsync(Guid calendarId, string accessToken, string blockId, CancellationToken ct = default);
}