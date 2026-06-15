using flatshare_server.Infrastructure.Model;
using flatshare_server.Infrastructure.Model.Bookings;
using flatshare_server.Infrastructure.Model.Responses.Booking;
using flatshare_server.Infrastructure.Repositories;
using flatshare_server.Infrastructure.Services.Bookings;
using flatshare_server.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class PaymentController
(
    PaymentService paymentService
)
    : ControllerBase
{
    [Authorize]
    [HttpGet("{paymentId}")]
    public async Task<ActionResult<PaymentDTO>> GetById([FromRoute] Guid paymentId)
    {
        var userId = AuthService.GetUserId(User);
        var dto = await paymentService.GetById(paymentId, userId);
        return Ok(dto);
    }

    [Authorize]
    [HttpGet]
    public async Task<ActionResult<PaymentDTO>> GetByBookingId([FromQuery] Guid bookingId)
    {
        var userId = AuthService.GetUserId(User);
        var dto = await paymentService.GetByBookingId(bookingId, userId);
        return Ok(dto);
    }
}