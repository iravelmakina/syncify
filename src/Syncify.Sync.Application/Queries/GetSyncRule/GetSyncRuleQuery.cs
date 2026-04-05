using MediatR;
using Syncify.Shared;
using Syncify.Sync.Application.DTOs;

namespace Syncify.Sync.Application.Queries.GetSyncRule;

public sealed record GetSyncRuleQuery(Guid RuleId) : IRequest<Result<SyncRuleResponse>>;