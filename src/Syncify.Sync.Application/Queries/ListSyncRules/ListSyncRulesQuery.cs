using MediatR;
using Syncify.Shared;
using Syncify.Shared.Results;
using Syncify.Sync.Application.Responses;

namespace Syncify.Sync.Application.Queries.ListSyncRules;

public sealed record ListSyncRulesQuery(UserId UserId) : IRequest<Result<IReadOnlyList<SyncRuleResponse>>>;
