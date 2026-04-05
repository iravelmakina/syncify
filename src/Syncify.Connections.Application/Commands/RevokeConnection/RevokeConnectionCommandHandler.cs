using MediatR;
using Syncify.Connections.Application.Ports;
using Syncify.Shared;

namespace Syncify.Connections.Application.Commands.RevokeConnection;

public sealed class RevokeConnectionCommandHandler(ICalendarAccountRepository repository)
    : IRequestHandler<RevokeConnectionCommand, Result<Unit>>
{
    public async Task<Result<Unit>> Handle(RevokeConnectionCommand request, CancellationToken ct)
    {
        var account = await repository.GetByIdAsync(request.AccountId, ct);
        if (account is null)
            return Result<Unit>.Failure(new ApplicationError.NotFound("CalendarAccount", request.AccountId));

        account.Revoke(DateTime.UtcNow);
        await repository.UpdateAsync(account, ct);

        return Result<Unit>.Success(Unit.Value);
    }
}