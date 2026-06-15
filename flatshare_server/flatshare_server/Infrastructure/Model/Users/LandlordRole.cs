using flatshare_server.Infrastructure.Services;

namespace flatshare_server.Infrastructure.Model.Users;

public class LandlordRole : UserRole
{
    private TenantCriteria _tenantCriteria = null!;
    public required TenantCriteria TenantCriteria
    {
        get => _tenantCriteria;
        init => _tenantCriteria = value;
    }

    protected override string GetStringRepresentation()
    {
        return AuthService.LandlordRole;
    }
}
