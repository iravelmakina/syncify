namespace Syncify.Sync.Infrastructure.Google;

public sealed class GoogleSyncOptions
{
    public const string SectionName = "Google";

    public string CalendarEventsEndpoint { get; init; } = "https://www.googleapis.com/calendar/v3/calendars/{calendarId}/events";
    public int InitialSyncLookbackDays { get; init; } = 30;
}
