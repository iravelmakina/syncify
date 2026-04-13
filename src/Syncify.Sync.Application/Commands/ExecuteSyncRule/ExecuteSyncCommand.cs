using MediatR;
using Syncify.Shared.Results;

namespace Syncify.Sync.Application.Commands.ExecuteSyncRule;

public sealed record ExecuteSyncCommand(Guid RuleId) : IRequest<Result<Unit>>;
