namespace flatshare_server.Infrastructure.Model.User;

public abstract class UserRole
{
    public User User { get; init; } = null!;
}
