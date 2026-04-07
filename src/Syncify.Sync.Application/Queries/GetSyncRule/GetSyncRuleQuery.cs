using MediatR;
using Syncify.Shared.Results;
using Syncify.Sync.Application.Responses;

namespace Syncify.Sync.Application.Queries.GetSyncRule;

public sealed record GetSyncRuleQuery(Guid RuleId) : IRequest<Result<SyncRuleResponse>>;
