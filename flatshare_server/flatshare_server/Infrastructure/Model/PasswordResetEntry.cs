namespace flatshare_server.Infrastructure.Model;
public class PasswordResetEntry
{
    public int Id { get; init; }
    public required Guid UserId { get; init; }
    public required string ResetCode { get; init; }
    public bool IsValid { get; init; } = true;

    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
    public DateTime ExpiresAt { get; init; } = DateTime.UtcNow.AddDays(1);
}

