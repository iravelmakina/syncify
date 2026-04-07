namespace Syncify.Connections.Application.DTOs;

public sealed record ConnectionResponse(Guid Id, string Provider, string Email, string Status, DateTime CreatedAt);