using flatshare_server.Infrastructure.Services;

namespace flatshare_server.Infrastructure.Model.Users;

public class AdminRole : UserRole
{
    protected override string GetStringRepresentation()
    {
        return AuthService.AdminRole;
    }
}
