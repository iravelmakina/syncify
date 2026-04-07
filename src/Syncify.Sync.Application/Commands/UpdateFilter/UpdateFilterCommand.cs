using MediatR;
using Syncify.Shared.Results;
using Syncify.Sync.Domain.ValueObjects;

namespace Syncify.Sync.Application.Commands.UpdateFilter;

public sealed record UpdateFilterCommand(Guid RuleId, FilterPolicy FilterPolicy) : IRequest<Result<Unit>>;
