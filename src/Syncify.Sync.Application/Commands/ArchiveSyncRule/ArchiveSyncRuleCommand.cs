using MediatR;
using Syncify.Shared.Results;

namespace Syncify.Sync.Application.Commands.ArchiveSyncRule;

public sealed record ArchiveSyncRuleCommand(Guid RuleId) : IRequest<Result<Unit>>;
