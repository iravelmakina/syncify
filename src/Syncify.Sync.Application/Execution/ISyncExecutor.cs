using MediatR;
using Syncify.Shared.Results;

namespace Syncify.Sync.Application.Execution;

public interface ISyncExecutor
{
    Task<Result<Unit>> ExecuteRuleAsync(Guid ruleId, CancellationToken ct = default);
}
