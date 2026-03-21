using flatshare_server.Infrastructure.Model.Requests;
using EmailValidation;
using flatshare_server.Infrastructure.Utils;
using flatshare_server.Infrastructure.Model.Responses;

namespace flatshare_server.Infrastructure.Model.User;

public class User
{
    public required Guid Id { get; init; }
    public required string FirstName { get; init; }
    public required string LastName { get; init; }
    public required string Email { get; init; }
    public required string PassHash { get; init; }

    // Zmiana z { get; init; } na { get; set; } (lub private set), 
    // abyśmy mogli modyfikować status w przyszłości i obejść problem z cyklicznym tworzeniem roli
    public AccountStatus Status { get; set; } = null!;
    public UserRole Role { get; private set; } = null!;

    public static User TryCreate(CreateUserRequest request)
    {
        var errors = new List<FieldError>();

        if (string.IsNullOrWhiteSpace(request.FirstName) || request.FirstName.Length < 3)
            errors.Add(new FieldError("firstName", "First name must be at least 3 characters long."));

        if (string.IsNullOrWhiteSpace(request.LastName) || request.LastName.Length < 3)
            errors.Add(new FieldError("lastName", "Last name must be at least 3 characters long."));

        if (string.IsNullOrWhiteSpace(request.Email) || !EmailValidator.Validate(request.Email))
            errors.Add(new FieldError("email", $"Invalid email address: {request.Email}"));

        if (string.IsNullOrWhiteSpace(request.Password) || request.Password.Length < 8)
            errors.Add(new FieldError("password", "Password must be at least 8 characters long."));

        if (errors.Any())
        {
            throw ErrorResponse.Generate("Register Error", StatusCodes.Status400BadRequest, errors);
        }

        var user = new User
        {
            Id = Guid.NewGuid(),
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            PassHash = Crypto.Sha256String(request.Password),
            Status = new AccountStatus { Value = AccountStatus.Type.Active }
        };

        user.Role = new TenantRole { User = user, TenantPreferences = new TenantPreferences() };

        return user;
    }
}