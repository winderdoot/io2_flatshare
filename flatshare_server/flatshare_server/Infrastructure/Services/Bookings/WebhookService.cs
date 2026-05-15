using flatshare_server.Infrastructure.Configuration;
using flatshare_server.Infrastructure.Extensions;
using flatshare_server.Infrastructure.Model.Responses;
using flatshare_server.Infrastructure.Repositories;
using Microsoft.Extensions.Options;
using Stripe;
using Stripe.Checkout;

namespace flatshare_server.Infrastructure.Services.Bookings;

public record WebhookInitRequest(string Secret);

public class WebhookService
(
    StripeWebhookSecretProvider secretProvider,
    IWebHostEnvironment env,
    IOptions<StripeOptions> stripeOpts,
    PaymentService paymentService
)
{
    public const string CheckoutSessionCompleted = "checkout.session.completed";
    public const string CheckoutSessionExpired = "checkout.session.expired";

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

    public async Task HandleStripeWebhook(string payload, string sigHeader)
    {
        try
        {
            /* Verify that event comes from stripe */
            var stripeEvent = EventUtility.ConstructEvent(
                payload,
                sigHeader,
                secretProvider.Secret
            );

            /* Handle the successful payment */
            if (stripeEvent.Type == WebhookService.CheckoutSessionCompleted)
            {
                var session = stripeEvent.Data.Object as Session;
                Guid? bookingId = session?.GetMetadataAs<Guid>("BookingId");
                if (bookingId is null)
                {
                    throw ErrorResponse.Generate("BookingId not found in session metadata", StatusCodes.Status400BadRequest);
                }

                await paymentService.GatewayConfirmedAsync(bookingId.Value, session.Id);

                Console.WriteLine($"Payment successful for Booking: {bookingId}");
            }
            /* Handle expired/cancelled sessions */
            else if (stripeEvent.Type == WebhookService.CheckoutSessionExpired)
            {
                var session = stripeEvent.Data.Object as Session;
                Guid? bookingId = session?.GetMetadataAs<Guid>("BookingId");
                if (bookingId is null)
                {
                    throw ErrorResponse.Generate("BookingId not found in session metadata", StatusCodes.Status400BadRequest);
                }
                await paymentService.UserAbortedAsync(bookingId.Value);
            }

            return;
        }
        catch (StripeException e)
        {
            throw ErrorResponse.Generate(e.Message, StatusCodes.Status400BadRequest);
        }
    }
}
