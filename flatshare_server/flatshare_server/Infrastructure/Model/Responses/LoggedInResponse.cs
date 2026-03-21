namespace flatshare_server.Infrastructure.Model.Responses;

public record LoggedInResponse(string Token, Guid SessionId, string Type, int ExpiresIn, List<string> Roles);
