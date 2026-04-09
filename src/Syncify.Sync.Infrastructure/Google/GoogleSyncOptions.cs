namespace Syncify.Sync.Infrastructure.Google;

public sealed class GoogleSyncOptions
{
    public const string SectionName = "Google";

    public string ApiBaseUrl { get; init; } = "https://www.googleapis.com";
    public string CalendarEventsPathTemplate { get; init; } = "/calendar/v3/calendars/{calendarId}/events";
    public int InitialSyncLookbackDays { get; init; } = 7;
}
