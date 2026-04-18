using Microsoft.EntityFrameworkCore;
using Syncify.Connections.Application.Ports;
using Syncify.Connections.Infrastructure.Persistence;
using Syncify.Shared;
using Syncify.Shared.Enums;
using Syncify.Shared.Ports;

namespace Syncify.Connections.Infrastructure.Services;

internal sealed class ConnectionService : IConnectionService
{
    private readonly ConnectionsDbContext _db;
    private readonly IOAuthProvider _oauthProvider;

    public ConnectionService(
        ConnectionsDbContext db,
        IOAuthProvider oauthProvider)
    {
        _db = db;
        _oauthProvider = oauthProvider;
    }

    public async Task<CalendarAccess> GetCalendarAccessAsync(Guid calendarId, UserId? userId = null, CancellationToken ct = default)
    {
        var calendar = await _db.Calendars
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == calendarId, ct)
            ?? throw new InvalidOperationException($"Calendar {calendarId} not found.");

        return Enum.Parse<CalendarAccess>(calendar.Access, ignoreCase: true);
    }

    public async Task<ProviderCalendarAccessToken> GetProviderCalendarAccessTokenAsync(Guid calendarId, UserId? userId = null, CancellationToken ct = default)
    {
        var calendar = await _db.Calendars
            .AsNoTracking()
            .Include(c => c.Account)
            .FirstOrDefaultAsync(c => c.Id == calendarId, ct)
            ?? throw new InvalidOperationException($"Calendar {calendarId} not found.");

        var accessToken = await _oauthProvider.RefreshAccessTokenAsync(calendar.Account.RefreshToken, ct);

        return new ProviderCalendarAccessToken(accessToken, calendar.ProviderCalendarId);
    }
}
