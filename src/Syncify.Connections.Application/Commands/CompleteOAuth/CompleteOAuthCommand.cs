using MediatR;
using Syncify.Shared;

namespace Syncify.Connections.Application.Commands.CompleteOAuth;

public sealed record CompleteOAuthCommand(UserId UserId, string Code) : IRequest<Result<Guid>>;