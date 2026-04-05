using Syncify.Connections.Domain.Aggregates;
using Syncify.Shared;

namespace Syncify.Connections.Application.Ports;

public interface ICalendarAccountRepository
{
    Task<CalendarAccount?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task CreateAsync(CalendarAccount account, CancellationToken ct = default);
    Task UpdateAsync(CalendarAccount account, CancellationToken ct = default);
    Task<IReadOnlyList<CalendarAccount>> ListByUserAsync(UserId userId, CancellationToken ct = default);
}