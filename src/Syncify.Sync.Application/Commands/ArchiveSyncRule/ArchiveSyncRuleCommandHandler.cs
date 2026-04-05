using MediatR;
using Syncify.Shared;
using Syncify.Sync.Application.Ports;

namespace Syncify.Sync.Application.Commands.ArchiveSyncRule;

public sealed class ArchiveSyncRuleCommandHandler(ISyncRuleRepository repository)
    : IRequestHandler<ArchiveSyncRuleCommand, Result<Unit>>
{
    public async Task<Result<Unit>> Handle(ArchiveSyncRuleCommand request, CancellationToken ct)
    {
        var rule = await repository.GetByIdAsync(request.RuleId, ct);
        if (rule is null)
            return Result<Unit>.Failure(new ApplicationError.NotFound("SyncRule", request.RuleId));

        rule.Archive(DateTime.UtcNow);
        await repository.UpdateAsync(rule, ct);

        return Result<Unit>.Success(Unit.Value);
    }
}