using System.Text.Json.Serialization;

namespace Syncify.Sync.Infrastructure.Google.Models;

internal sealed record GoogleEventsResponse(
    [property: JsonPropertyName("items")] List<GoogleEventItem> Items,
    [property: JsonPropertyName("nextSyncToken")] string? NextSyncToken);

internal sealed record GoogleEventItem(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("status")] string? Status,
    [property: JsonPropertyName("summary")] string? Summary,
    [property: JsonPropertyName("start")] GoogleEventDateTime? Start,
    [property: JsonPropertyName("end")] GoogleEventDateTime? End,
    [property: JsonPropertyName("updated")] DateTime Updated);

internal sealed record GoogleEventDateTime(
    [property: JsonPropertyName("dateTime")] string? DateTime,
    [property: JsonPropertyName("date")] string? Date);
