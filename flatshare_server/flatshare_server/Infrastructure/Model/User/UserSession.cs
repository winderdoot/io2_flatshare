namespace flatshare_server.Infrastructure.Model.User;

public class UserSession
{
    public required Guid Id { get; init; }
    public required Guid UserId { get; init; }
}
