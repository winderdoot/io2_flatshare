using flatshare_server.Infrastructure.Model.Users;

namespace flatshare_server.Infrastructure.Model.Requests;

public record class CreateUserRequest(string FirstName, string LastName, string Email, string Password, string Role)
{
    public const string Tenant = "TENANT";
    public const string Landlord = "LANDLORD";
}

