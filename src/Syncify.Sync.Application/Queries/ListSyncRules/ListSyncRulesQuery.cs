using MediatR;
using Syncify.Shared;
using Syncify.Sync.Application.DTOs;

namespace Syncify.Sync.Application.Queries.ListSyncRules;

public sealed record ListSyncRulesQuery(UserId UserId) : IRequest<Result<IReadOnlyList<SyncRuleResponse>>>;