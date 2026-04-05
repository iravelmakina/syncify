using MediatR;
using Syncify.Connections.Application.Ports;
using Syncify.Connections.Domain.Aggregates;
using Syncify.Connections.Domain.Enums;
using Syncify.Connections.Domain.ValueObjects;
using Syncify.Shared;

namespace Syncify.Connections.Application.Commands.CompleteOAuth;

public sealed class CompleteOAuthCommandHandler(
    ICalendarAccountRepository repository,
    IOAuthProvider oAuthProvider)
    : IRequestHandler<CompleteOAuthCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CompleteOAuthCommand command, CancellationToken ct)
    {
        var result = await oAuthProvider.ExchangeCodeAsync(command.Code, ct);

        var credential = new OAuthCredential(result.RefreshToken, result.TokenExpiresAt);
        var account = CalendarAccount.Create(command.UserId, Provider.Google, credential, DateTime.UtcNow);

        await repository.CreateAsync(account, ct);
        return Result<Guid>.Success(account.Id);
    }
}