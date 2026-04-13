namespace Syncify.Connections.Api.Responses;

public sealed record ConnectionResponse(Guid Id, string Provider, string Email, string Status, DateTime CreatedAt);

public sealed record CalendarResponse(Guid Id, string ProviderCalendarId, string Name, string Access);

public sealed record AuthUrlResponse(string Url);

public sealed record CalendarAccessResponse(string Access);

public sealed record FreshTokenResponse(string AccessToken);
