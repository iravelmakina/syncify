using System.Net.Http.Json;
using System.Web;
using Microsoft.Extensions.Options;
using Syncify.Connections.Application.DTOs;
using Syncify.Connections.Application.Ports;
using Syncify.Connections.Infrastructure.Google.Models;

namespace Syncify.Connections.Infrastructure.Google;

public sealed class GoogleOAuthProvider : IOAuthProvider
{
    private readonly HttpClient _httpClient;
    private readonly GoogleOptions _options;

    public GoogleOAuthProvider(HttpClient httpClient, IOptions<GoogleOptions> options)
    {
        _httpClient = httpClient;
        _options = options.Value;
    }

    public string GenerateAuthUrl()
    {
        var builder = new UriBuilder(_options.AuthEndpoint);
        var query = HttpUtility.ParseQueryString(string.Empty);

        query["client_id"] = _options.ClientId;
        query["redirect_uri"] = _options.RedirectUri;
        query["response_type"] = "code";
        query["scope"] = _options.CalendarScope;
        query["access_type"] = "offline";
        query["prompt"] = "consent";

        builder.Query = query.ToString();
        return builder.Uri.ToString();
    }

    public async Task<OAuthResult> ExchangeCodeAsync(string code, CancellationToken ct = default)
    {
        var payload = new Dictionary<string, string>
        {
            ["code"] = code,
            ["client_id"] = _options.ClientId,
            ["client_secret"] = _options.ClientSecret,
            ["redirect_uri"] = _options.RedirectUri,
            ["grant_type"] = "authorization_code"
        };

        var tokenResponse = await RequestTokenAsync(payload, ct);

        return new OAuthResult(
            tokenResponse.AccessToken,
            tokenResponse.RefreshToken ?? throw new InvalidOperationException("Google did not return a refresh token."),
            DateTimeOffset.UtcNow.AddSeconds(tokenResponse.ExpiresIn));
    }

    public async Task<string> RefreshAccessTokenAsync(string refreshToken, CancellationToken ct = default)
    {
        var payload = new Dictionary<string, string>
        {
            ["refresh_token"] = refreshToken,
            ["client_id"] = _options.ClientId,
            ["client_secret"] = _options.ClientSecret,
            ["grant_type"] = "refresh_token"
        };

        var tokenResponse = await RequestTokenAsync(payload, ct);
        return tokenResponse.AccessToken;
    }

    private async Task<GoogleTokenResponse> RequestTokenAsync(
        Dictionary<string, string> payload,
        CancellationToken ct)
    {
        using var content = new FormUrlEncodedContent(payload);
        var response = await _httpClient.PostAsync(_options.TokenEndpoint, content, ct);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync(ct);
            throw new HttpRequestException(
                $"Google token request failed with {response.StatusCode}: {error}");
        }

        return await response.Content.ReadFromJsonAsync<GoogleTokenResponse>(ct)
            ?? throw new InvalidOperationException("Failed to deserialize Google token response.");
    }
}
