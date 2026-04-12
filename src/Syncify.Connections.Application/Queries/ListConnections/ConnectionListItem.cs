namespace Syncify.Connections.Application.Queries.ListConnections;

public sealed record ConnectionListItem(Guid Id, string Provider, string Email, string Status, DateTime CreatedAt);