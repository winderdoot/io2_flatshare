namespace flatshare_server.Infrastructure.Model.User;

public class TenantRole : UserRole
{
    private TenantPreferences _preferences = null!;
    public required TenantPreferences TenantPreferences
    {
        get => _preferences;
        init => _preferences = value;
    }
}
