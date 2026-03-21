using flatshare_server.Infrastructure.Model;

namespace flatshare_server.Infrastructure.Repositories;
public interface ISessionRepository
{
    public Task SaveNew(Guid sessionId, Guid userId);
    public Task<UserSession> GetBySessionId(Guid sessionId);
}
