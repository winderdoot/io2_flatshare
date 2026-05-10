namespace flatshare_server.Infrastructure.Model.Responses;

public record UserDTO(Guid Id, string FirstName, string LastName, string Email, string Role);

public record UserCreatedResponse(string Message, UserDTO User);
