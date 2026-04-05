namespace Syncify.Sync.Application.DTOs;

public sealed record CalendarChange(
    string EventId,
    string? Title,
    DateTime? Start,
    DateTime? End,
    bool IsCancelled,
    DateTime UpdatedAt);

public sealed record FetchChangesResult(
    IReadOnlyList<CalendarChange> Changes,
    string? NewCursor);