using MediatR;
using Syncify.Shared;
using Syncify.Shared.Results;
using Syncify.Sync.Domain.ValueObjects;

namespace Syncify.Sync.Application.Commands.CreateSyncRule;

public sealed record CreateSyncRuleCommand(
    UserId UserId,
    Guid SourceCalendarId,
    Guid TargetCalendarId,
    bool CopyTitle,
    string CustomTitle,
    FilterPolicy FilterPolicy) : IRequest<Result<Guid>>;
