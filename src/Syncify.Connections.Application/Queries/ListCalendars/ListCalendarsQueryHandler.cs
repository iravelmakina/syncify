using MediatR;
using Syncify.Connections.Application.Ports;
using Syncify.Connections.Domain.ValueObjects;
using Syncify.Shared.Errors;
using Syncify.Shared.Results;

namespace Syncify.Connections.Application.Queries.ListCalendars;

public sealed class ListCalendarsQueryHandler(
    ICalendarAccountRepository repository,
    IOAuthProvider oAuthProvider,
    ICalendarProvider calendarProvider)
    : IRequestHandler<ListCalendarsQuery, Result<IReadOnlyList<CalendarInfo>>>
{
    public async Task<Result<IReadOnlyList<CalendarInfo>>> Handle(ListCalendarsQuery request, CancellationToken ct)
    {
        var account = await repository.GetByIdAsync(request.AccountId, ct);
        if (account is null)
            return Result<IReadOnlyList<CalendarInfo>>.Failure(
                new ApplicationError.NotFound("CalendarAccount", request.AccountId));

        account.CheckExpiry(DateTime.UtcNow);

        var accessToken = await oAuthProvider.RefreshAccessTokenAsync(account.Credential.RefreshToken, ct);
        var providerCalendars = await calendarProvider.ListCalendarsAsync(accessToken, ct);

        var calendars = providerCalendars.Select(pc =>
        {
            var existing = account.Calendars.FirstOrDefault(
                c => c.ProviderCalendarId == pc.ProviderCalendarId);
            return new CalendarInfo(
                existing?.Id ?? Guid.NewGuid(),
                pc.ProviderCalendarId,
                pc.Name,
                pc.Access);
        }).ToList();

        account.RefreshCalendars(calendars, DateTime.UtcNow);
        await repository.UpdateAsync(account, ct);

        return Result<IReadOnlyList<CalendarInfo>>.Success(account.Calendars);
    }
}
