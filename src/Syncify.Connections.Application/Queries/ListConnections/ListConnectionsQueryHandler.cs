using MediatR;
using Syncify.Connections.Application.Ports;
using Syncify.Shared.Results;

namespace Syncify.Connections.Application.Queries.ListConnections;

public sealed class ListConnectionsQueryHandler(ICalendarAccountRepository repository)
    : IRequestHandler<ListConnectionsQuery, Result<IReadOnlyList<ConnectionListItem>>>
{
    public async Task<Result<IReadOnlyList<ConnectionListItem>>> Handle(ListConnectionsQuery request, CancellationToken ct)
    {
        var accounts = await repository.ListByUserAsync(request.UserId, ct);

        var items = accounts.Select(a => new ConnectionListItem(
            a.Id,
            a.Provider.ToString(),
            a.Email,
            a.Status.ToString(),
            a.CreatedAt)).ToList();

        return Result<IReadOnlyList<ConnectionListItem>>.Success(items);
    }
}
