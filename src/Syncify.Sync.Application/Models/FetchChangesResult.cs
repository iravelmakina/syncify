namespace Syncify.Sync.Application.Models;

public record FetchChangesResult(
    string? NewCursor,
    IReadOnlyList<CalendarEventDto> ChangedEvents,
    IReadOnlyList<string> DeletedEventIds,
    string? TimeZone);

public record CalendarEventDto(
    string Id,
    string? Title,
    DateTime Start,
    DateTime End,
    DateTime UpdatedAt);
    