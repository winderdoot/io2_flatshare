using flatshare_server.Infrastructure.Model.Requests.Booking;
using flatshare_server.Infrastructure.Model.Responses.Booking;
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
    AuthService authService,
    PaymentService paymentService
) : Controller
{
    [Authorize(Roles = AuthService.TenantRole)]
    [HttpPost]
    public async Task<ActionResult<BookingCreatedResponse>> CreateBooking([FromBody] CreateBookingRequest request)
    {
        var userId = authService.GetUserId(User);
        return await bookingService.Create(request, userId);
    }

    [Authorize(Roles = AuthService.LandlordRole)]
    [HttpPost("{bookingId}/accept")]
    public async Task<ActionResult<AcceptBookingResponse>> AcceptBooking([FromRoute] Guid bookingId)
    {
        var ownerId = authService.GetUserId(User);
        var resp = await bookingService.Accept(bookingId, ownerId);
        return Ok(resp);
    }

    [Authorize(Roles = AuthService.LandlordRole)]
    [HttpPost("{bookingId}/reject")]
    public async Task<ActionResult<RejectBookingResponse>> RejectBooking([FromRoute] Guid bookingId, [FromBody] RejectBookingRequest request)
    {
        var userId = authService.GetUserId(User);
        var resp = await bookingService.Reject(bookingId, userId, request);
        return Ok(resp);
    }

    [Authorize(Roles = AuthService.TenantRole)]
    [HttpPost("{bookingId}/cancel")]
    public async Task<ActionResult<CancelBookingResponse>> CancelBooking([FromRoute] Guid bookingId, [FromBody] CancelBookingRequest request)
    {
        var userId = authService.GetUserId(User);
        var resp = await bookingService.Cancel(bookingId, userId, request);
        return Ok(resp);
    }

    [Authorize(Roles = AuthService.TenantRole)]
    [HttpPost("{bookingId:guid}/pay")]
    public async Task<ActionResult<PaymentInitiatedResponse>> PayBooking([FromRoute] Guid bookingId, [FromBody] PayBookingRequest request)
    {
        var userId = authService.GetUserId(User);
        var resp = await bookingService.InitiatePayment(bookingId, userId, request);
        return Ok(resp);
    }

    [Authorize]
    [HttpGet("{bookingId}")]
    public async Task<ActionResult<BookingDTO>> GetBooking([FromRoute] Guid bookingId)
    {
        var userId = authService.GetUserId(User);
        var resp = await bookingService.GetById(bookingId, userId);
        return Ok(resp);
    }

    [Authorize]
    [HttpGet("{bookingId}/pay")]
    public async Task<ActionResult<PaymentInitiatedResponse>> StartPayment([FromRoute] Guid bookingId)
    {
        return Ok(await paymentService.InitiatePayment(bookingId));
    }
}