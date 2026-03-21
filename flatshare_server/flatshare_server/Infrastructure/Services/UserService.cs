using flatshare_server.Infrastructure.Model.Requests;
using flatshare_server.Infrastructure.Model.Responses;
using flatshare_server.Infrastructure.Model.User;
using flatshare_server.Infrastructure.Repositories;

namespace flatshare_server.Infrastructure.Services;

public class UserService
{
    private IUserRepository _repo;
    public UserService(IUserRepository userRepo)
    {
        _repo = userRepo;
    }
    public async Task<UserDTO> Create(CreateUserRequest request)
    {
        User user = User.TryCreate(request);
        await _repo.SaveNew(user);
        return new UserDTO(user.Id, user.FirstName, user.LastName, user.Email);
    }
    public async Task<UserDTO> GetById(Guid id)
    {
        User user = await _repo.GetById(id);

        return new UserDTO(user.Id, user.FirstName, user.LastName, user.Email);
    }
}
