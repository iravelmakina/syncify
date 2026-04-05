using Syncify.Connections.Application.DTOs;

namespace Syncify.Connections.Application.Ports;

public interface IOAuthProvider
{
    string GenerateAuthUrl();
    Task<OAuthResult> ExchangeCodeAsync(string code, CancellationToken ct = default);
    Task<string> RefreshAccessTokenAsync(string refreshToken, CancellationToken ct = default);
}