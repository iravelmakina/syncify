namespace Syncify.Sync.Application.DTOs;

public record FetchChangesResult(
    string? NewCursor,
    IReadOnlyList<CalendarEventDto> ChangedEvents,
    IReadOnlyList<string> DeletedEventIds);

public record CalendarEventDto(
    string Id,
    string? Title,
    DateTime Start,
    DateTime End,
    DateTime UpdatedAt);
    