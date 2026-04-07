using MediatR;
using Syncify.Connections.Application.Responses;
using Syncify.Shared.Results;

namespace Syncify.Connections.Application.Queries.ListCalendars;

public sealed record ListCalendarsQuery(Guid AccountId) : IRequest<Result<IReadOnlyList<CalendarResponse>>>;
