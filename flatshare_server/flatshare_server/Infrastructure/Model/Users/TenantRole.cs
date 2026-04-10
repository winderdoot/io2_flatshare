using flatshare_server.Infrastructure.Services;

namespace flatshare_server.Infrastructure.Model.Users;

public class TenantRole : UserRole
{
    private TenantPreferences _preferences = null!;
    public required TenantPreferences TenantPreferences
    {
        get => _preferences;
        init => _preferences = value;
    }

    protected override string GetStringRepresentation()
    {
        return AuthService.TenantRole;
    }
}
