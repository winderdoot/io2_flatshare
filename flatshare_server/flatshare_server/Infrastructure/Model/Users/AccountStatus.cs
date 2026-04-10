namespace flatshare_server.Infrastructure.Model;

/* No, this will not be a hierarchy of classes that represent different account states.
 * I've done that before and it sucked. Enum is better and serializes far better.
 * I will die on this hill.
 */
public class AccountStatus
{
    public enum Type
    {
        Active, 
        ResetRequested, 
        Blocked, 
        Deleted
    }

    public Type Value { get; set; } = Type.Active;
    /* Reason for blocking */
    public string? Reason { get; set; } = null;
}
