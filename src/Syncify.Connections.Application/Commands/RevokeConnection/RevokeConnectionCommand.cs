using MediatR;
using Syncify.Shared;

namespace Syncify.Connections.Application.Commands.RevokeConnection;

public sealed record RevokeConnectionCommand(Guid AccountId) : IRequest<Result<Unit>>;