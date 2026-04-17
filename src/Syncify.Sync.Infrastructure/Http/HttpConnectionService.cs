using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Syncify.Shared;
using Syncify.Shared.Contracts;
using Syncify.Shared.Enums;
using Syncify.Shared.Middleware;
using Syncify.Shared.Ports;

namespace Syncify.Sync.Infrastructure.Http;

internal sealed class HttpConnectionService : IConnectionService
{
    private readonly HttpClient _httpClient;
    private readonly ConnectionsServiceOptions _options;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public HttpConnectionService(
        HttpClient httpClient,
        IOptions<ConnectionsServiceOptions> options,
        IHttpContextAccessor httpContextAccessor)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<CalendarAccess> GetCalendarAccessAsync(Guid calendarId, CancellationToken ct = default)
    {
        var url = $"/internal/calendars/{calendarId}/access";

        var response = await SendGetAsync(url, ct);

        if (response.StatusCode == HttpStatusCode.NotFound)
            throw new InvalidOperationException($"Calendar {calendarId} not found.");

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync(ct);
            throw new HttpRequestException(
                $"Connections Service access request failed with {response.StatusCode}: {error}");
        }

        var result = await response.Content.ReadFromJsonAsync<CalendarAccessResponse>(ct)
            ?? throw new InvalidOperationException("Failed to deserialize calendar access response.");

        return Enum.Parse<CalendarAccess>(result.Access, ignoreCase: true);
    }

    public async Task<ProviderCalendarAccessToken> GetProviderCalendarAccessTokenAsync(Guid calendarId, CancellationToken ct = default)
    {
        var url = $"/internal/calendars/{calendarId}/token";

        var response = await SendGetAsync(url, ct);

        if (response.StatusCode == HttpStatusCode.NotFound)
            throw new InvalidOperationException($"Calendar {calendarId} not found.");

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync(ct);
            throw new HttpRequestException(
                $"Connections Service token request failed with {response.StatusCode}: {error}");
        }

        var result = await response.Content.ReadFromJsonAsync<ProviderCalendarAccessTokenResponse>(ct)
            ?? throw new InvalidOperationException("Failed to deserialize provider calendar access token response.");

        return new ProviderCalendarAccessToken(result.AccessToken, result.ProviderCalendarId);
    }

    private async Task<HttpResponseMessage> SendGetAsync(string url, CancellationToken ct)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, url);

        var userId = TryGetUserId();
        if (userId is not null)
            request.Headers.Add("X-User-ID", userId.Value.ToString());

        return await _httpClient.SendAsync(request, ct);
    }

    private UserId? TryGetUserId()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext is null)
            return null;

        return httpContext.Items.TryGetValue(UserIdMiddleware.UserIdKey, out var userId)
            && userId is UserId typedUserId
                ? typedUserId
                : null;
    }
}
