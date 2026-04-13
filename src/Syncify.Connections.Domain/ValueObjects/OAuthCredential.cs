using Syncify.Shared.Errors;

namespace Syncify.Connections.Domain.ValueObjects;

public sealed record OAuthCredential
{
    public string RefreshToken { get; }
    public DateTime TokenExpiresAt { get; }

    public OAuthCredential(string refreshToken, DateTime tokenExpiresAt)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
            throw new DomainException("Refresh token cannot be empty.");

        RefreshToken = refreshToken;
        TokenExpiresAt = tokenExpiresAt;
    }

    public bool IsExpired(DateTime utcNow) => TokenExpiresAt <= utcNow;
}
