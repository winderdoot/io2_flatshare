namespace flatshare_server.Infrastructure.Configuration;

public record class StripeOptions
{
    public static string OptionsKey = "StripeOptions";

    public required string SecretKey { get; set; }
    public required string PublishableKey { get; set; }
    public required string WebhookSecretInitKey { get; set; }
}
