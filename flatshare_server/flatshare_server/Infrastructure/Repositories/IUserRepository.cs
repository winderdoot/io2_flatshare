using flatshare_server.Infrastructure.Model.Requests;
using flatshare_server.Infrastructure.Model.User;

namespace flatshare_server.Infrastructure.Repositories;

public interface IUserRepository
{
    public Task SaveNew(User user);
    public Task<User> GetById(Guid id);
}
