using Microsoft.EntityFrameworkCore;
using Syncify.Connections.Application.Ports;
using Syncify.Connections.Domain.Aggregates;
using Syncify.Connections.Domain.Enums;
using Syncify.Connections.Infrastructure.Persistence.Mappers;
using Syncify.Shared;

namespace Syncify.Connections.Infrastructure.Persistence;

public class CalendarAccountRepository : ICalendarAccountRepository
{
    private readonly ConnectionsDbContext _db;

    public CalendarAccountRepository(ConnectionsDbContext db)
    {
        _db = db;
    }

    public async Task<CalendarAccount?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await _db.CalendarAccounts
            .AsNoTracking()
            .Include(a => a.Calendars)
            .FirstOrDefaultAsync(a => a.Id == id, ct);

        return entity?.ToDomain();
    }

    public async Task CreateAsync(CalendarAccount account, CancellationToken ct = default)
    {
        var entity = account.ToEntity();
        _db.CalendarAccounts.Add(entity);
        await _db.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(CalendarAccount account, CancellationToken ct = default)
    {
        var entity = account.ToEntity();
        _db.CalendarAccounts.Update(entity);
        await _db.SaveChangesAsync(ct);
    }

    public async Task<IReadOnlyList<CalendarAccount>> ListByUserAsync(UserId userId, CancellationToken ct = default)
    {
        var entities = await _db.CalendarAccounts
            .AsNoTracking()
            .Include(a => a.Calendars)
            .Where(a => a.UserId == userId.Value)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync(ct);

        return entities.Select(e => e.ToDomain()).ToList();
    }
}
