using Microsoft.AspNetCore.Mvc;
using flatshare_server.Infrastructure.Model.Responses;
using flatshare_server.Infrastructure.Services;
using flatshare_server.Infrastructure.Model.Exceptions;
using flatshare_server.Infrastructure.Model.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using flatshare_server.Infrastructure.Model.Users;
using flatshare_server.Infrastructure.Model;
using flatshare_server.Infrastructure.Model.Requests.Listing;
using flatshare_server.Infrastructure.Services.Bookings;
using flatshare_server.Infrastructure.Model.Requests.Booking;

namespace flatshare_server.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize(Roles = AuthService.AdminRole)]
public class AdminController
    (
        UserService userService,
        ListingService listingService,
        PaymentService paymentService,
        BookingService bookingService
    ) : Controller
{
    [HttpPost("users/{userId}/ban")]
    public async Task<IActionResult> BanUser([FromRoute] Guid userId, [FromBody] BanUserRequest banRequest)
    {
        // ban user
        var newStatus = new AccountStatus { Value = AccountStatus.Type.Blocked, Reason = banRequest.Reason };
        await userService.UpdateStatusByIdAsync(userId, newStatus);

        // hide listings
        await listingService.BatchModerationHideByUserIdAsync(userId);
        var bookings = await bookingService.Get(userId, null);

        foreach (var booking in bookings)
        {
            // cancel bookings
            await bookingService.AdminForceCancel(booking.Id, userId);

            // give refund
            if (booking.StartDate < DateOnly.FromDateTime(DateTime.UtcNow)) // jeśli już rozpoczęte to nie zwracamy? idk czy to ma sens
                continue;
            await paymentService.InitiateBookingRefund(booking.Id);
        }

        throw new NotImplementedException();

        return Ok();
    }
}
