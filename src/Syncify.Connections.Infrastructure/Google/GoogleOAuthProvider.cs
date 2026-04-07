using System.Net.Http.Json;
using System.Web;
using Google.Apis.Auth;
using Microsoft.Extensions.Options;
using Syncify.Connections.Application.DTOs;
using Syncify.Connections.Application.Ports;
using Syncify.Connections.Infrastructure.Google.Models;

namespace Syncify.Connections.Infrastructure.Google;

internal sealed class GoogleOAuthProvider : IOAuthProvider
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
        var builder = new UriBuilder(new Uri(new Uri(_options.AccountsBaseUrl, UriKind.Absolute), _options.OAuthAuthPath));
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
        var identity = await ValidateIdTokenAsync(tokenResponse.IdToken);

        return new OAuthResult(
            tokenResponse.RefreshToken ?? throw new InvalidOperationException("Google did not return a refresh token."),
            DateTime.UtcNow.AddSeconds(tokenResponse.ExpiresIn),
            identity.ProviderAccountId,
            identity.Email);
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
        var response = await _httpClient.PostAsync(_options.OAuthTokenPath, content, ct);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync(ct);
            throw new HttpRequestException(
                $"Google token request failed with {response.StatusCode}: {error}");
        }

        return await response.Content.ReadFromJsonAsync<GoogleTokenResponse>(ct)
            ?? throw new InvalidOperationException("Failed to deserialize Google token response.");
    }

    private async Task<GoogleIdentity> ValidateIdTokenAsync(string? idToken)
    {
        if (string.IsNullOrWhiteSpace(idToken))
            throw new InvalidOperationException("Google did not return an id_token.");

        try
        {
            var payload = await GoogleJsonWebSignature.ValidateAsync(
                idToken,
                new GoogleJsonWebSignature.ValidationSettings
                {
                    Audience = [_options.ClientId]
                });

            if (string.IsNullOrWhiteSpace(payload.Subject))
                throw new InvalidOperationException("Google id_token is missing the sub claim.");

            if (string.IsNullOrWhiteSpace(payload.Email))
                throw new InvalidOperationException("Google id_token is missing the email claim.");

            return new GoogleIdentity(payload.Subject, payload.Email);
        }
        catch (InvalidJwtException ex)
        {
            throw new InvalidOperationException("Google returned an invalid id_token.", ex);
        }
    }

    private sealed record GoogleIdentity(string ProviderAccountId, string Email);
}
