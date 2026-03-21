using flatshare_server.Infrastructure.Model.Exceptions;
using flatshare_server.Infrastructure.Model.Requests;
using flatshare_server.Infrastructure.Model.Responses;
using flatshare_server.Infrastructure.Model.User;
using flatshare_server.Infrastructure.Repositories;
using flatshare_server.Infrastructure.Utils;

namespace flatshare_server.Infrastructure.Services
{
    public class AuthService
    {
        private IUserRepository _repo;
        public AuthService(IUserRepository userRepo)
        {
            _repo = userRepo;
        }

        public async Task<string> Authenticate(string email, string password)
        {
            User? user = await _repo.GetByEmail(email);

            if (user is null)
                throw ErrorResponse.Generate(
                    $"Wrong email or password",
                    StatusCodes.Status401Unauthorized
                    );

            if (!PasswordEncoder.Matches(password, user.PassHash, user.Id))
                throw ErrorResponse.Generate(
                    $"Wrong email or password",
                    StatusCodes.Status401Unauthorized
                    );

            throw new NotImplementedException();
        }
    }
}
