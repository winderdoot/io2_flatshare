using flatshare_server.Infrastructure.Model.Requests.Booking;
using flatshare_server.Infrastructure.Model.Responses.Booking;
using flatshare_server.Infrastructure.Model.Users;
using flatshare_server.Infrastructure.Services;
using flatshare_server.Infrastructure.Services.Bookings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;

namespace flatshare_server.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class BookingsController
(
    BookingService bookingService,
    PaymentService paymentService
) : Controller
{
    [Authorize(Roles = AuthService.TenantRole)]
    [HttpPost]
    public async Task<ActionResult<BookingCreatedResponse>> CreateBooking([FromBody] CreateBookingRequest request)
    {
        var userId = AuthService.GetUserId(User);
        return await bookingService.Create(request, userId);
    }

    [Authorize(Roles = AuthService.LandlordRole)]
    [HttpPost("{bookingId}/accept")]
    public async Task<ActionResult<AcceptBookingResponse>> AcceptBooking([FromRoute] Guid bookingId)
    {
        var ownerId = AuthService.GetUserId(User);
        var resp = await bookingService.Accept(bookingId, ownerId);
        return Ok(resp);
    }

    [Authorize(Roles = AuthService.LandlordRole)]
    [HttpPost("{bookingId}/reject")]
    public async Task<ActionResult<RejectBookingResponse>> RejectBooking([FromRoute] Guid bookingId, [FromBody] RejectBookingRequest request)
    {
        var userId = AuthService.GetUserId(User);
        var resp = await bookingService.Reject(bookingId, userId, request);
        return Ok(resp);
    }

    [Authorize(Roles = AuthService.TenantRole)]
    [HttpPost("{bookingId}/cancel")]
    public async Task<ActionResult<CancelBookingResponse>> CancelBooking([FromRoute] Guid bookingId, [FromBody] CancelBookingRequest request)
    {
        var userId = AuthService.GetUserId(User);
        var resp = await bookingService.Cancel(bookingId, userId, request);
        return Ok(resp);
    }

    [Authorize(Roles = AuthService.TenantRole)]
    [HttpPost("{bookingId}/pay")]
    public async Task<ActionResult<PaymentInitiatedResponse>> PayBooking([FromRoute] Guid bookingId, [FromBody] PayBookingRequest request)
    {
        return Ok(await paymentService.InitiatePayment(bookingId, request));
    }

    [Authorize]
    [HttpGet("{bookingId}")]
    public async Task<ActionResult<BookingDTO>> GetBooking([FromRoute] Guid bookingId)
    {
        var userId = AuthService.GetUserId(User);
        var resp = await bookingService.GetById(bookingId, userId);
        return Ok(resp);
    }

    [Authorize]
    [HttpGet]
    public async Task<ActionResult<List<BookingDTO>>> GetByQuery([FromQuery] Guid? tenantId, [FromQuery] Guid? listingId)
    {
        var userId = authService.GetUserId(User);

        if (User.IsInRole(AuthService.TenantRole))
        {
            return Ok(await bookingService.Get(tenantId, listingId));
        }

        if (User.IsInRole(AuthService.LandlordRole))
        {
            if (!listingId.HasValue)
            {
                return BadRequest("listingId is required for landlord booking queries");
            }
            return Ok(await bookingService.GetForListingOwner(listingId.Value, userId));
        }

        return Forbid();
    }

    [Authorize]
    [HttpGet("{bookingId}/me")]
    public async Task<ActionResult<List<BookingDTO>>> GetUserBookings()
    {
        return Ok(await bookingService.GetUserBookingAsync(User));
    }
}