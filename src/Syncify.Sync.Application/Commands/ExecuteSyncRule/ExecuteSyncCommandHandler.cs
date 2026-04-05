using MediatR;
using Syncify.Shared;
using Syncify.Sync.Application.Services;

namespace Syncify.Sync.Application.Commands.ExecuteSyncRule;

public sealed class ExecuteSyncCommandHandler(SyncExecutor syncExecutor)
    : IRequestHandler<ExecuteSyncCommand, Result<Unit>>
{
    public async Task<Result<Unit>> Handle(ExecuteSyncCommand request, CancellationToken ct)
    {
        await syncExecutor.ExecuteRuleAsync(request.RuleId, ct);
        return Result<Unit>.Success(Unit.Value);
    }
}