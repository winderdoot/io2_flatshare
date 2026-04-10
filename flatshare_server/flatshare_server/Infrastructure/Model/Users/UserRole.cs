namespace flatshare_server.Infrastructure.Model.Users;

public abstract class UserRole
{
    public User User { get; init; } = null!; 

    public sealed override string ToString()
    {
        return GetStringRepresentation();
    }
    protected abstract string GetStringRepresentation();
}
