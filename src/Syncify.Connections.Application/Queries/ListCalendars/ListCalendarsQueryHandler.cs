using MediatR;
using Syncify.Connections.Application.DTOs;
using Syncify.Connections.Application.Ports;
using Syncify.Shared;

namespace Syncify.Connections.Application.Queries.ListCalendars;

public sealed class ListCalendarsQueryHandler(
    ICalendarAccountRepository repository,
    IOAuthProvider oAuthProvider,
    ICalendarProvider calendarProvider)
    : IRequestHandler<ListCalendarsQuery, Result<IReadOnlyList<CalendarResponse>>>
{
    public async Task<Result<IReadOnlyList<CalendarResponse>>> Handle(ListCalendarsQuery request, CancellationToken ct)
    {
        var account = await repository.GetByIdAsync(request.AccountId, ct);
        if (account is null)
            return Result<IReadOnlyList<CalendarResponse>>.Failure(
                new ApplicationError.NotFound("CalendarAccount", request.AccountId));

        account.CheckExpiry(DateTime.UtcNow);

        var accessToken = await oAuthProvider.RefreshAccessTokenAsync(account.Credential.RefreshToken, ct);
        var calendars = await calendarProvider.ListCalendarsAsync(accessToken, ct);

        account.RefreshCalendars(calendars, DateTime.UtcNow);
        await repository.UpdateAsync(account, ct);

        var response = calendars.Select(c => new CalendarResponse(
            c.Id,
            c.ProviderCalendarId,
            c.Name,
            c.Access.ToString())).ToList();

        return Result<IReadOnlyList<CalendarResponse>>.Success(response);
    }
}