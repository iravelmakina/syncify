using MediatR;
using Syncify.Shared;

namespace Syncify.Sync.Application.Commands.UpdateTitle;

public sealed record UpdateTitleCommand(Guid RuleId, bool CopyTitle, string CustomTitle) : IRequest<Result<Unit>>;