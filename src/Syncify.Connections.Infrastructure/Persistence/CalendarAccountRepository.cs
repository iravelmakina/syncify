using Microsoft.EntityFrameworkCore;
using Syncify.Connections.Application.Ports;
using Syncify.Connections.Domain.Aggregates;
using Syncify.Connections.Infrastructure.Persistence.Mappers;
using Syncify.Shared;

namespace Syncify.Connections.Infrastructure.Persistence;

internal sealed class CalendarAccountRepository : ICalendarAccountRepository
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
        var entity = await _db.CalendarAccounts
            .Include(a => a.Calendars)
            .FirstOrDefaultAsync(a => a.Id == account.Id, ct);

        if (entity == null)
            throw new InvalidOperationException($"Account {account.Id} not found.");

        var updated = account.ToEntity();
        
        _db.Entry(entity).CurrentValues.SetValues(updated);

        foreach (var existing in entity.Calendars.ToList())
        {
            if (updated.Calendars.All(c => c.Id != existing.Id))
                _db.Calendars.Remove(existing);
        }

        foreach (var @new in updated.Calendars)
        {
            var existing = entity.Calendars.FirstOrDefault(e => e.Id == @new.Id);
            if (existing != null)
                _db.Entry(existing).CurrentValues.SetValues(@new);
            else
                entity.Calendars.Add(@new);
        }

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
