namespace flatshare_server.Infrastructure.Services.Bookings;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Stripe;
using Stripe.Checkout;
using flatshare_server.Infrastructure.Configuration;
using flatshare_server.Infrastructure.Model.Bookings;
using flatshare_server.Infrastructure.Model.Requests.Booking;
using flatshare_server.Infrastructure.Model.Responses;
using flatshare_server.Infrastructure.Model.Responses.Booking;
using flatshare_server.Infrastructure.Repositories;
using flatshare_server.Infrastructure.Services;
using flatshare_server.Infrastructure.Model.Users;

public class PaymentService
(
    FlatshareDbContext dbContext,
    IStripeClient stripeClient,
    ListingService listingService,
    UserService userService
)
{
    public async Task<PaymentInitiatedResponse> InitiatePayment(Guid bookingId, PayBookingRequest request)
    {
        var booking = await dbContext.Bookings.FindAsync(bookingId);
        if (booking is null)
        {
            throw ErrorResponse.Generate("Booking not found", StatusCodes.Status404NotFound);
        }
        if (!booking.IsPendingPayment)
        {
            throw ErrorResponse.Generate($"Cannot initiate payment in booking state '{booking.Status}'");
        }

        var payment = new Payment(bookingId, booking.TotalPrice);

        payment.RedirectToGateway();
        dbContext.Payments.Add(payment);
        await dbContext.SaveChangesAsync();

        var options = new SessionCreateOptions
        {
            SuccessUrl = request.ReturnUrl,
            CancelUrl = request.CancelUrl,
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

        var service = new SessionService(stripeClient);
        var session = await service.CreateAsync(options);

        return new PaymentInitiatedResponse(
            payment.PaymentId.ToString(),
            booking.BookingId.ToString(),
            "INITIATED",
            session.Url,
            booking.TotalPrice.Value,
            booking.TotalPrice.CurrencyStr()
        );
    }

    public async Task<PaymentDTO> GetById(Guid paymentId, Guid requesterUserId)
    {
        var payment = await dbContext.Payments.FindAsync(paymentId);
        if (payment is null)
            throw ErrorResponse.Generate("Payment not found", StatusCodes.Status404NotFound);

        var booking = await dbContext.Bookings.FindAsync(payment.BookingId);
        if (booking is null)
            throw ErrorResponse.Generate("Booking not found", StatusCodes.Status404NotFound);

        if (booking.TenantId == requesterUserId)
            return payment.IntoDTO();

        var listing = await listingService.GetByIdAsync(booking.ListingId, attachOwner: true);
        if (listing.Owner is not null && listing.Owner.Id == requesterUserId)
            return payment.IntoDTO();

        var requester = await userService.GetByIdAsync(requesterUserId);
        if (requester.Role.ToString() == AuthService.AdminRole)
            return payment.IntoDTO();

        throw ErrorResponse.Generate("Forbidden", StatusCodes.Status403Forbidden);
    }

    public async Task<PaymentDTO> GetByBookingId(Guid bookingId, Guid requesterUserId)
    {
        var booking = await dbContext.Bookings.FindAsync(bookingId);
        if (booking is null)
            throw ErrorResponse.Generate("Booking not found", StatusCodes.Status404NotFound);

        var payment = await dbContext.Payments
            .AsNoTracking()
            .Where(p => p.BookingId == bookingId)
            .OrderByDescending(p => p.PaymentId)
            .FirstOrDefaultAsync();

        if (payment is null)
            throw ErrorResponse.Generate("Payment not found", StatusCodes.Status404NotFound);

        if (booking.TenantId == requesterUserId)
            return payment.IntoDTO();

        var listing = await listingService.GetByIdAsync(booking.ListingId, attachOwner: true);
        if (listing.Owner is not null && listing.Owner.Id == requesterUserId)
            return payment.IntoDTO();

        var requester = await userService.GetByIdAsync(requesterUserId);
        if (requester.Role.ToString() == AuthService.AdminRole)
            return payment.IntoDTO();

        throw ErrorResponse.Generate("Forbidden", StatusCodes.Status403Forbidden);
    }
}
