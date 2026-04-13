using MediatR;
using Syncify.Shared.Results;
using Syncify.Sync.Domain.Aggregates;

namespace Syncify.Sync.Application.Queries.GetSyncRule;

public sealed record GetSyncRuleQuery(Guid RuleId) : IRequest<Result<SyncRule>>;
