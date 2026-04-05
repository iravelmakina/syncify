using MediatR;
using Syncify.Shared;

namespace Syncify.Sync.Application.Commands.ArchiveSyncRule;

public sealed record ArchiveSyncRuleCommand(Guid RuleId) : IRequest<Result<Unit>>;