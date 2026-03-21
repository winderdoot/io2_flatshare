namespace flatshare_server.Infrastructure.Model.Requests;

public record CreateUserRequest(string FirstName, string LastName, string Email, string Password);