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
        var oauthResult = await oAuthProvider.ExchangeCodeAsync(command.Code, ct);
        var newCredential = new OAuthCredential(oauthResult.RefreshToken, oauthResult.TokenExpiresAt);
        var utcNow = DateTime.UtcNow;

        var existingConnections = await repository.ListByUserAsync(command.UserId, ct);
        var existingAccount = existingConnections.FirstOrDefault(c =>
            c.Provider == Provider.Google && c.ProviderAccountId == oauthResult.ProviderAccountId);

        if (existingAccount is not null)
        {
            existingAccount.Reconnect(newCredential, utcNow);
            await repository.UpdateAsync(existingAccount, ct);
            return Result<Guid>.Success(existingAccount.Id);
        }

        var newAccount = CalendarAccount.Create(
            command.UserId, Provider.Google, oauthResult.ProviderAccountId, oauthResult.Email,
            newCredential, utcNow);
        await repository.CreateAsync(newAccount, ct);
        return Result<Guid>.Success(newAccount.Id);
    }
}
