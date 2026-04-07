using MediatR;
using Syncify.Connections.Application.DTOs;
using Syncify.Connections.Application.Ports;
using Syncify.Shared;

namespace Syncify.Connections.Application.Queries.ListConnections;

public sealed class ListConnectionsQueryHandler(ICalendarAccountRepository repository)
    : IRequestHandler<ListConnectionsQuery, Result<IReadOnlyList<ConnectionResponse>>>
{
    public async Task<Result<IReadOnlyList<ConnectionResponse>>> Handle(ListConnectionsQuery request, CancellationToken ct)
    {
        var accounts = await repository.ListByUserAsync(request.UserId, ct);

        var response = accounts.Select(a => new ConnectionResponse(
            a.Id,
            a.Provider.ToString(),
            a.Email,
            a.Status.ToString(),
            a.CreatedAt)).ToList();

        return Result<IReadOnlyList<ConnectionResponse>>.Success(response);
    }
}
