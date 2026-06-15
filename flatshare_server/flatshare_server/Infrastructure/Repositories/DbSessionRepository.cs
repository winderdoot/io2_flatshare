using flatshare_server.Infrastructure.Model.Responses;
using flatshare_server.Infrastructure.Model.Users;
using Microsoft.EntityFrameworkCore;

namespace flatshare_server.Infrastructure.Repositories;
public class DbSessionRepository : ISessionRepository
{
    private readonly FlatshareDbContext _context;
    public DbSessionRepository(FlatshareDbContext dbcontext)
    {
        _context = dbcontext;
    }

    public async Task SaveNew(Guid sessionId, Guid userId)
    {
        var exists = await _context.Sessions.AnyAsync(s => s.Id == sessionId);
        if (exists)
            throw ErrorResponse.Generate(
                "Could not create new session, id conflict",
                StatusCodes.Status409Conflict);

        _context.Sessions.Add(new UserSession { Id = sessionId, UserId = userId });
        await _context.SaveChangesAsync();
    }

    public async Task<UserSession> GetBySessionId(Guid sessionId)
    {
        var session = await _context.Sessions.FindAsync(sessionId);
        if (session is null || !session.IsValid)
        {
            throw ErrorResponse.Generate(
                $"Session with Id: '{sessionId}' doesn't exist",
                StatusCodes.Status404NotFound
            );
        }

        return session;
    }

    public async Task InvalidateByUserId(Guid userId)
    {
        var sessions = await _context.Sessions
            .Where(s => s.UserId == userId)
            .ToListAsync();

        foreach (var session in sessions)
        {
            session.IsValid = false;
        }

        await _context.SaveChangesAsync();
    }

    public async Task<bool> IsSessionValid(Guid sessionId)
    {
        var sess = await _context.Sessions
            .Where(s => s.Id == sessionId).FirstOrDefaultAsync();

        if (sess is null || !sess.IsValid)
            return false;
        return true;
    }
}
