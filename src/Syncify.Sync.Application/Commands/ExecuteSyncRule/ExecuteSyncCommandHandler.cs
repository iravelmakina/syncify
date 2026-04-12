using MediatR;
using Syncify.Shared.Results;
using Syncify.Sync.Application.Execution;

namespace Syncify.Sync.Application.Commands.ExecuteSyncRule;

public sealed class ExecuteSyncCommandHandler(ISyncExecutor syncExecutor)
    : IRequestHandler<ExecuteSyncCommand, Result<Unit>>
{
    public async Task<Result<Unit>> Handle(ExecuteSyncCommand request, CancellationToken ct)
    {
        await syncExecutor.ExecuteRuleAsync(request.RuleId, ct);
        return Result<Unit>.Success(Unit.Value);
    }
}
