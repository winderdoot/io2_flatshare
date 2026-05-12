namespace flatshare_server.Infrastructure.Configuration;

public record class StripeOptions
{
    public static string OptionsKey = "Stripe";

    public required string SecretKey { get; init; }
    public required string PublishableKey { get; init; }
}
