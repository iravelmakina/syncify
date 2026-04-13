using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.Options;
using Syncify.Shared.Contracts;
using Syncify.Shared.Enums;
using Syncify.Shared.Ports;

namespace Syncify.Sync.Infrastructure.Http;

internal sealed class HttpConnectionService : IConnectionService
{
    private readonly HttpClient _httpClient;
    private readonly ConnectionsServiceOptions _options;

    public HttpConnectionService(
        HttpClient httpClient,
        IOptions<ConnectionsServiceOptions> options)
    {
        _httpClient = httpClient;
        _options = options.Value;
    }

    public async Task<CalendarAccess> GetCalendarAccessAsync(Guid calendarId, CancellationToken ct = default)
    {
        var url = $"/internal/calendars/{calendarId}/access";

        var response = await _httpClient.GetAsync(url, ct);

        if (response.StatusCode == HttpStatusCode.NotFound)
            throw new InvalidOperationException($"Calendar {calendarId} not found.");

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync(ct);
            throw new HttpRequestException(
                $"Connections Service access request failed with {response.StatusCode}: {error}");
        }

        var result = await response.Content.ReadFromJsonAsync<InternalCalendarAccessResponse>(ct)
            ?? throw new InvalidOperationException("Failed to deserialize calendar access response.");

        return Enum.Parse<CalendarAccess>(result.Access, ignoreCase: true);
    }

    public async Task<ProviderCalendarAccessToken> GetProviderCalendarAccessTokenAsync(Guid calendarId, CancellationToken ct = default)
    {
        var url = $"/internal/calendars/{calendarId}/token";

        var response = await _httpClient.GetAsync(url, ct);

        if (response.StatusCode == HttpStatusCode.NotFound)
            throw new InvalidOperationException($"Calendar {calendarId} not found.");

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync(ct);
            throw new HttpRequestException(
                $"Connections Service token request failed with {response.StatusCode}: {error}");
        }

        var result = await response.Content.ReadFromJsonAsync<InternalProviderCalendarAccessTokenResponse>(ct)
            ?? throw new InvalidOperationException("Failed to deserialize provider calendar access token response.");

        return new ProviderCalendarAccessToken(result.AccessToken, result.ProviderCalendarId);
    }
}
