using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.Extensions.Options;
using Syncify.Connections.Application.Ports;
using Syncify.Connections.Domain.ValueObjects;
using Syncify.Connections.Infrastructure.Google.Models;

namespace Syncify.Connections.Infrastructure.Google;

internal sealed class GoogleCalendarProvider : ICalendarProvider
{
    private readonly HttpClient _httpClient;
    private readonly GoogleOptions _options;

    public GoogleCalendarProvider(HttpClient httpClient, IOptions<GoogleOptions> options)
    {
        _httpClient = httpClient;
        _options = options.Value;
    }

    public async Task<IReadOnlyList<CalendarInfo>> ListCalendarsAsync(
        string accessToken,
        CancellationToken ct = default)
    {
        var url = _options.CalendarListEndpoint;

        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        var response = await _httpClient.SendAsync(request, ct);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync(ct);
            throw new HttpRequestException(
                $"Google Calendar list request failed with {response.StatusCode}: {error}");
        }

        var result = await response.Content.ReadFromJsonAsync<GoogleCalendarListResponse>(ct)
            ?? throw new InvalidOperationException("Failed to deserialize Google Calendar list response.");

        return result.Items
            .Select(item => new CalendarInfo(
                Guid.NewGuid(),
                item.Id,
                item.Summary ?? item.Id,
                GoogleAccessRoleMapper.ToDomain(item.AccessRole)))
            .ToList();
    }
}
