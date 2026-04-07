namespace Syncify.Connections.Infrastructure.Google;

public sealed class GoogleOptions
{
    public const string SectionName = "Google";

    public string ClientId { get; init; } = string.Empty;
    public string ClientSecret { get; init; } = string.Empty;
    public string RedirectUri { get; init; } = "urn:ietf:wg:oauth:2.0:oob";
    public string AccountsBaseUrl { get; init; } = "https://accounts.google.com";
    public string OAuthBaseUrl { get; init; } = "https://oauth2.googleapis.com";
    public string OAuthAuthPath { get; init; } = "/o/oauth2/v2/auth";
    public string OAuthTokenPath { get; init; } = "/token";
    public string ApiBaseUrl { get; init; } = "https://www.googleapis.com";
    public string CalendarListPath { get; init; } = "/calendar/v3/users/me/calendarList";
    public string CalendarScope { get; init; } = "https://www.googleapis.com/auth/calendar openid email";
}
