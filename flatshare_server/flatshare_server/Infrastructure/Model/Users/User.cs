using Microsoft.AspNetCore.StaticAssets;
using EmailValidation;
using flatshare_server.Infrastructure.Utils;
using flatshare_server.Infrastructure.Model.Exceptions;
using flatshare_server.Infrastructure.Model.Responses;
using Microsoft.AspNetCore.Http;
using flatshare_server.Infrastructure.Model.Requests;

namespace flatshare_server.Infrastructure.Model.Users;

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

    public static User TryCreate(CreateUserRequest request)
    {
        var errors = new List<FieldError>();

        if (string.IsNullOrEmpty(request.FirstName) || request.FirstName.Length < 3)
        {
            errors.Add(new (nameof(request.FirstName), "First name must be at least 3 characters long."));
        }
        if (string.IsNullOrEmpty(request.LastName) || request.LastName.Length < 3)
        {
            errors.Add(new(nameof(request.LastName), "Last name must be at least 3 characters long."));
        }
        if (string.IsNullOrEmpty(request.Email) || !EmailValidator.Validate(request.Email))
        {
            errors.Add(new (nameof(request.Email), $"Invalid email address: {request.Email}"));
        }
        if (string.IsNullOrEmpty(request.Password) || request.Password.Length < 8)
        {
            errors.Add(new (nameof(request.Password), $"Password must be at least 8 characters long"));
        }
        if (string.IsNullOrEmpty(request.Role) || (request.Role != CreateUserRequest.Tenant && request.Role != CreateUserRequest.Landlord))
        {
            errors.Add(new(nameof(request.Role), $"Role must be equal to {CreateUserRequest.Tenant} or {CreateUserRequest.Landlord}"));
        }

        if (errors.Any())
        {
            throw ErrorResponse.Generate("Register Error", StatusCodes.Status400BadRequest, errors);
        }

        Guid guid = Guid.NewGuid();
        var user = new User
        {
            Id = guid,
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            PassHash = PasswordEncoder.Encrypt(request.Password, guid),
            Status = new AccountStatus { },
            Role = null!
        };

        if (request.Role == CreateUserRequest.Tenant)
        {
            user._role = new TenantRole { User = user, TenantPreferences = new TenantPreferences { } };
        }
        else
        {
            user._role = new LandlordRole { User = user, TenantCriteria = new TenantCriteria { } };
        }

        return user;
    }
    public UserDTO IntoDTO()
    {
        return new UserDTO(Id, FirstName, LastName, Email, Role.ToString());
    }
}
