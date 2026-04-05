using MediatR;
using Syncify.Connections.Application.DTOs;
using Syncify.Shared;

namespace Syncify.Connections.Application.Queries.ListCalendars;

public sealed record ListCalendarsQuery(Guid AccountId) : IRequest<Result<IReadOnlyList<CalendarResponse>>>;