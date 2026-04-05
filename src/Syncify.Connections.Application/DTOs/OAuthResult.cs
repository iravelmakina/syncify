namespace Syncify.Connections.Application.DTOs;

public sealed record OAuthResult(string RefreshToken, DateTime TokenExpiresAt);