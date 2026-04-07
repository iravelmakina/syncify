using MediatR;
using Syncify.Connections.Application.Responses;
using Syncify.Shared;
using Syncify.Shared.Results;

namespace Syncify.Connections.Application.Queries.ListConnections;

public sealed record ListConnectionsQuery(UserId UserId) : IRequest<Result<IReadOnlyList<ConnectionResponse>>>;
