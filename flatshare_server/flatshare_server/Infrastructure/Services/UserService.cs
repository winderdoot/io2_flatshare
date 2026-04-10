using flatshare_server.Infrastructure.Model.Requests;
using flatshare_server.Infrastructure.Model.Responses;
using flatshare_server.Infrastructure.Model.Users;
using flatshare_server.Infrastructure.Repositories;

namespace flatshare_server.Infrastructure.Services;

public class UserService
{
    private IUserRepository _repo;
    public UserService(IUserRepository userRepo)
    {
        _repo = userRepo;
    }
    public async Task<User> Create(CreateUserRequest request)
    {
        User user = User.TryCreate(request);
        await _repo.SaveNew(user);   
        return user;
    }
    public async Task<User> GetByIdAsync(Guid id)
    {
        return await _repo.GetById(id);
    }
}
