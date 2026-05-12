namespace flatshare_server.Infrastructure.Configuration;

public record class StripeOptions
{
    public static string OptionsKey = "StripeOptions";

    public required string SecretKey { get; init; }
    public required string PublishableKey { get; init; }
}
