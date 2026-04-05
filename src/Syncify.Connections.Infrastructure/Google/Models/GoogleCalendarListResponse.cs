using System.Text.Json.Serialization;

namespace Syncify.Connections.Infrastructure.Google.Models;

internal sealed record GoogleCalendarListResponse(
    [property: JsonPropertyName("items")] List<GoogleCalendarEntry> Items);

internal sealed record GoogleCalendarEntry(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("summary")] string? Summary,
    [property: JsonPropertyName("accessRole")] string AccessRole);
