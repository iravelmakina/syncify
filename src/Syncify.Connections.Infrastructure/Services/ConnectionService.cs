using Microsoft.EntityFrameworkCore;
using Syncify.Connections.Application.Ports;
using Syncify.Connections.Infrastructure.Persistence;
using Syncify.Shared;

namespace Syncify.Connections.Infrastructure.Services;

public sealed class ConnectionService : IConnectionService
{
    private readonly ConnectionsDbContext _db;
    private readonly IOAuthProvider _oauthProvider;
    private readonly ITokenEncryptor _encryptor;

    public ConnectionService(
        ConnectionsDbContext db,
        IOAuthProvider oauthProvider,
        ITokenEncryptor encryptor)
    {
        _db = db;
        _oauthProvider = oauthProvider;
        _encryptor = encryptor;
    }

    public async Task<CalendarAccess> GetCalendarAccessAsync(Guid calendarId, CancellationToken ct = default)
    {
        var calendar = await _db.Calendars
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == calendarId, ct)
            ?? throw new InvalidOperationException($"Calendar {calendarId} not found.");

        return Enum.Parse<CalendarAccess>(calendar.Access, ignoreCase: true);
    }

    public async Task<string> GetFreshAccessTokenAsync(Guid calendarId, CancellationToken ct = default)
    {
        var calendar = await _db.Calendars
            .AsNoTracking()
            .Include(c => c.Account)
            .FirstOrDefaultAsync(c => c.Id == calendarId, ct)
            ?? throw new InvalidOperationException($"Calendar {calendarId} not found.");

        var decryptedRefreshToken = _encryptor.Decrypt(calendar.Account.RefreshTokenEnc);
        var accessToken = await _oauthProvider.RefreshAccessTokenAsync(decryptedRefreshToken, ct);

        return accessToken;
    }
}
