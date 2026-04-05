using System.Text.Json.Serialization;

namespace Syncify.Connections.Infrastructure.Google.Models;

internal sealed record GoogleTokenResponse(
    [property: JsonPropertyName("access_token")] string AccessToken,
    [property: JsonPropertyName("refresh_token")] string? RefreshToken,
    [property: JsonPropertyName("expires_in")] int ExpiresIn);
