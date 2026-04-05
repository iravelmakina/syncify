using MediatR;
using Syncify.Connections.Application.DTOs;
using Syncify.Shared;

namespace Syncify.Connections.Application.Queries.ListConnections;

public sealed record ListConnectionsQuery(UserId UserId) : IRequest<Result<IReadOnlyList<ConnectionResponse>>>;