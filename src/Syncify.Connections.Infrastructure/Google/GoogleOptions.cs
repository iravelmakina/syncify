namespace Syncify.Connections.Infrastructure.Google;

public sealed class GoogleOptions
{
    public const string SectionName = "Google";

    public string ClientId { get; init; } = string.Empty;
    public string ClientSecret { get; init; } = string.Empty;
    public string RedirectUri { get; init; } = "urn:ietf:wg:oauth:2.0:oob";
    public string AuthEndpoint { get; init; } = "https://accounts.google.com/o/oauth2/v2/auth";
    public string TokenEndpoint { get; init; } = "https://oauth2.googleapis.com/token";
    public string CalendarListEndpoint { get; init; } = "https://www.googleapis.com/calendar/v3/users/me/calendarList";
    public string CalendarScope { get; init; } = "https://www.googleapis.com/auth/calendar openid email";
}
