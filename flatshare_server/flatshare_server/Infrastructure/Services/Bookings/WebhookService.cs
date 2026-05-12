using flatshare_server.Infrastructure.Configuration;
using flatshare_server.Infrastructure.Model.Responses;
using Microsoft.Extensions.Options;

namespace flatshare_server.Infrastructure.Services.Bookings;

public record WebhookInitRequest(string Secret);

public class WebhookService
(
    StripeWebhookSecretProvider secretProvider,
    IWebHostEnvironment env,
    IOptions<StripeOptions> stripeOpts
)
{
    public void InitializeSecret(string secret, string initKey)
    {
        if (!env.IsDevelopment())
            throw ErrorResponse.Generate("Only needed in local development. In production stripe servers can access deployed application.", StatusCodes.Status403Forbidden);

        var expected = stripeOpts.Value.WebhookSecretInitKey;

        if (string.IsNullOrEmpty(initKey) || initKey != expected)
        {
            throw ErrorResponse.Generate("Invalid Initialization Key.", StatusCodes.Status401Unauthorized);
        }

        secretProvider.Secret = secret;
    }
}
