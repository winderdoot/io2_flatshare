namespace flatshare_server.Infrastructure.Services.Bookings;

using flatshare_server.Infrastructure.Configuration;
using flatshare_server.Infrastructure.Model.Bookings;
using flatshare_server.Infrastructure.Model.Responses;
using flatshare_server.Infrastructure.Model.Responses.Booking;
using flatshare_server.Infrastructure.Repositories;
using Microsoft.AspNetCore.Mvc;
using Stripe;
using Stripe.Checkout;

public class PaymentService
(
    FlatshareDbContext dbContext,
    SessionService stripeService
)
{
    public async Task<PaymentInitiatedResponse> InitiatePayment(Guid bookingId)
    {
        var booking = await dbContext.Bookings.FindAsync(bookingId);
        if (booking is null)
        {
            throw ErrorResponse.Generate("Booking not found", StatusCodes.Status404NotFound);
        }
        var payment = new Payment(bookingId, booking.TotalPrice);
        payment.RedirectToGateway();

        var options = new SessionCreateOptions
        {
            SuccessUrl = $"{ApiRoutes.ServerUrl}/ap/v1/payments/{payment.PaymentId}/success",
            CancelUrl = $"{ApiRoutes.ServerUrl}/ap/v1/payments/{payment.PaymentId}/cancel",
            LineItems = new List<SessionLineItemOptions>
            {
                new SessionLineItemOptions
                {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        /* Stripe uses subunits (grosze or euro cents) */
                        UnitAmount = (long)(booking.TotalPrice.Value * 100),
                        Currency = booking.TotalPrice.CurrencyStr().ToLower(),
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = $"Booking for Listing {booking.ListingId}",
                        },
                    },
                    Quantity = 1,
                },
            },
            Mode = "payment",
            ClientReferenceId = booking.BookingId.ToString()
        };

        var session = await stripeService.CreateAsync(options);

        return new PaymentInitiatedResponse(
            payment.PaymentId.ToString(),
            booking.BookingId.ToString(),
            "INITIATED",
            session.Url,
            booking.TotalPrice.Value,
            booking.TotalPrice.CurrencyStr()
        );
    }
}
