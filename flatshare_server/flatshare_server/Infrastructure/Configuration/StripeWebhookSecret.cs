using Microsoft.AspNetCore.Mvc;

namespace flatshare_server.Infrastructure.Configuration;

public class StripeWebhookSecretProvider
{
    public required string Secret { get; set; }
}