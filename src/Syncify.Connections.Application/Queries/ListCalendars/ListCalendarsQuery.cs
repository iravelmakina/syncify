using MediatR;
using Syncify.Connections.Domain.ValueObjects;
using Syncify.Shared.Results;

namespace Syncify.Connections.Application.Queries.ListCalendars;

public sealed record ListCalendarsQuery(Guid AccountId) : IRequest<Result<IReadOnlyList<CalendarInfo>>>;
