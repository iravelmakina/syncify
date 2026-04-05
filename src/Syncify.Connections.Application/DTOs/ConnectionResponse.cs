namespace Syncify.Connections.Application.DTOs;

public sealed record ConnectionResponse(Guid Id, string Provider, string Status, DateTime CreatedAt);