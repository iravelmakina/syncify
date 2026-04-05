using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Web;
using Microsoft.Extensions.Options;
using Syncify.Sync.Application.DTOs;
using Syncify.Sync.Application.Ports;
using Syncify.Sync.Infrastructure.Google.Models;

namespace Syncify.Sync.Infrastructure.Google;

public sealed class GoogleCalendarSyncer : ICalendarSyncer
{
    private readonly HttpClient _httpClient;
    private readonly GoogleSyncOptions _options;

    public GoogleCalendarSyncer(HttpClient httpClient, IOptions<GoogleSyncOptions> options)
    {
        _httpClient = httpClient;
        _options = options.Value;
    }

    public async Task<FetchChangesResult> FetchChangesAsync(
        Guid calendarId,
        string accessToken,
        string? cursor,
        CancellationToken ct = default)
    {
        var url = BuildEventsUrl(calendarId, cursor);

        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        var response = await _httpClient.SendAsync(request, ct);

        if (response.StatusCode == HttpStatusCode.Gone)
        {
            // syncToken expired — caller should clear cursor and mappings, then full re-sync
            return new FetchChangesResult(null, [], []);
        }

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync(ct);
            throw new HttpRequestException(
                $"Google Calendar events request failed with {response.StatusCode}: {error}");
        }

        var result = await response.Content.ReadFromJsonAsync<GoogleEventsResponse>(ct)
            ?? throw new InvalidOperationException("Failed to deserialize Google Calendar events response.");

        var changed = new List<CalendarEventDto>();
        var deleted = new List<string>();

        foreach (var item in result.Items)
        {
            if (GoogleEventStatusMapper.IsCancelled(item.Status))
            {
                deleted.Add(item.Id);
            }
            else if (item.Start is not null && item.End is not null)
            {
                changed.Add(new CalendarEventDto(
                    item.Id,
                    item.Summary,
                    ParseDateTime(item.Start),
                    ParseDateTime(item.End),
                    item.Updated));
            }
        }

        return new FetchChangesResult(result.NextSyncToken, changed, deleted);
    }

    public async Task<string> CreateBlockAsync(
        Guid calendarId,
        string accessToken,
        string title,
        DateTime start,
        DateTime end,
        CancellationToken ct = default)
    {
        var url = BuildEventsUrl(calendarId);
        var body = BuildEventBody(title, start, end);

        using var request = new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = new StringContent(body, Encoding.UTF8, "application/json")
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        var response = await _httpClient.SendAsync(request, ct);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync(ct);
            throw new HttpRequestException(
                $"Google Calendar create event failed with {response.StatusCode}: {error}");
        }

        var created = await response.Content.ReadFromJsonAsync<GoogleEventItem>(ct)
            ?? throw new InvalidOperationException("Failed to deserialize created event response.");

        return created.Id;
    }

    public async Task UpdateBlockAsync(
        Guid calendarId,
        string accessToken,
        string blockId,
        string title,
        DateTime start,
        DateTime end,
        CancellationToken ct = default)
    {
        var url = $"{BuildEventsUrl(calendarId)}/{Uri.EscapeDataString(blockId)}";
        var body = BuildEventBody(title, start, end);

        using var request = new HttpRequestMessage(HttpMethod.Patch, url)
        {
            Content = new StringContent(body, Encoding.UTF8, "application/json")
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        var response = await _httpClient.SendAsync(request, ct);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync(ct);
            throw new HttpRequestException(
                $"Google Calendar update event failed with {response.StatusCode}: {error}");
        }
    }

    public async Task DeleteBlockAsync(
        Guid calendarId,
        string accessToken,
        string blockId,
        CancellationToken ct = default)
    {
        var url = $"{BuildEventsUrl(calendarId)}/{Uri.EscapeDataString(blockId)}";

        using var request = new HttpRequestMessage(HttpMethod.Delete, url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        var response = await _httpClient.SendAsync(request, ct);

        if (!response.IsSuccessStatusCode && response.StatusCode != HttpStatusCode.NotFound)
        {
            var error = await response.Content.ReadAsStringAsync(ct);
            throw new HttpRequestException(
                $"Google Calendar delete event failed with {response.StatusCode}: {error}");
        }
    }

    private string BuildEventsUrl(Guid calendarId, string? syncToken = null)
    {
        var baseUrl = _options.CalendarEventsEndpoint
            .Replace("{calendarId}", Uri.EscapeDataString(calendarId.ToString()));

        var builder = new UriBuilder(baseUrl);
        var query = HttpUtility.ParseQueryString(string.Empty);

        if (syncToken is not null)
        {
            query["syncToken"] = syncToken;
        }
        else
        {
            query["timeMin"] = DateTime.UtcNow.AddDays(-_options.InitialSyncLookbackDays).ToString("o");
            query["singleEvents"] = "true";
        }

        builder.Query = query.ToString();
        return builder.Uri.ToString();
    }

    private static string BuildEventBody(string title, DateTime start, DateTime end)
    {
        var eventObj = new
        {
            summary = title,
            start = new { dateTime = start.ToString("o"), timeZone = "UTC" },
            end = new { dateTime = end.ToString("o"), timeZone = "UTC" }
        };

        return JsonSerializer.Serialize(eventObj);
    }

    private static DateTime ParseDateTime(GoogleEventDateTime dt)
    {
        if (dt.DateTime is not null)
            return System.DateTime.Parse(dt.DateTime).ToUniversalTime();

        if (dt.Date is not null)
            return System.DateTime.Parse(dt.Date).ToUniversalTime();

        throw new InvalidOperationException("Google event has no dateTime or date.");
    }
}
