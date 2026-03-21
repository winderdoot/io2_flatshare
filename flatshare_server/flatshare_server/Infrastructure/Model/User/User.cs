using flatshare_server.Infrastructure.Model.Requests;
using Microsoft.AspNetCore.StaticAssets;
using EmailValidation;
using flatshare_server.Infrastructure.Utils;
using flatshare_server.Infrastructure.Model.Exceptions;
using flatshare_server.Infrastructure.Model.Responses;
using Microsoft.AspNetCore.Http;

namespace flatshare_server.Infrastructure.Model.User;

public class User
{
    public required Guid Id { get; init; }
    public required string FirstName { get; init; }
    public required string LastName { get; init; }
    public required string Email { get; init; }

    private string _passHash = "";
    public required string PassHash 
    {
        get => _passHash;
        init => _passHash = value;
    }
    private AccountStatus _status = null!;
    public required AccountStatus Status 
    { 
        get => _status; 
        init => _status = value; 
    }
    private UserRole _role = null!;
    public required UserRole Role 
    {
        get => _role;
        init => _role = value; 
    }

    /* Skąd wiemy czy jest tworzony tenant czy landlord? */
    public static User TryCreate(CreateUserRequest request)
    {
        if (string.IsNullOrEmpty(request.FirstName) || request.FirstName.Length < 3)
        {
            throw ErrorResponse.Generate(
                "Register Error",
                fields: [new (nameof(request.FirstName), "First name must be at least 3 characters long.")]
            );
        }
        else if (string.IsNullOrEmpty(request.LastName) || request.LastName.Length < 3)
        {
            throw ErrorResponse.Generate(
                "Register Error",
                fields: [new (nameof(request.LastName), "Last name must be at least 3 characters long.")]
            );
        }
        else if (string.IsNullOrEmpty(request.Email) || !EmailValidator.Validate(request.Email))
        {
            throw ErrorResponse.Generate(
                "Register Error",
                fields: [new (nameof(request.Email), $"Invalid email address: {request.Email}")]
            );
        }
        else if (string.IsNullOrEmpty(request.Password) || request.Password.Length < 8)
        {
            throw ErrorResponse.Generate(
                "Register Error",
                fields: [new (nameof(request.Password), $"Password must be at least 8 characters long")]
            );
        }

        var user = new User
        {
            Id = Guid.NewGuid(),
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            PassHash = Crypto.Sha256String(request.Password),
            Status = new AccountStatus { }, /* TODO: This is only Tenant for now */
            Role = null!
        };
        if (request.Role != null && request.Role.Equals("LANDLORD", StringComparison.OrdinalIgnoreCase))
        {
            user._role = new LandlordRole
            {
                User = user,
                TenantCriteria = new TenantCriteria { }
            };
        }
        else
        {
            user._role = new TenantRole
            {
                User = user,
                TenantPreferences = new TenantPreferences { }
            };
        }

        return user;
    }
}
