using MediatR;
using Syncify.Shared.Results;

namespace Syncify.Sync.Application.Commands.ResumeSyncRule;

public sealed record ResumeSyncRuleCommand(Guid RuleId) : IRequest<Result<Unit>>;
