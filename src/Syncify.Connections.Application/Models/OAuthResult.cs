namespace Syncify.Connections.Application.Models;

public sealed record OAuthResult(string RefreshToken, DateTime TokenExpiresAt, string ProviderAccountId, string Email);
