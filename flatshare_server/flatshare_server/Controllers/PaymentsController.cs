using flatshare_server.Infrastructure.Model;
using flatshare_server.Infrastructure.Model.Bookings;
using flatshare_server.Infrastructure.Repositories;
using flatshare_server.Infrastructure.Services.Bookings;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class PaymentController
(
    PaymentService paymentService
)
    : ControllerBase
{
    [HttpGet("{paymentId}/success")]
    public async Task<IActionResult> PaymentSuccess([FromRoute] Guid paymentId)
    {
        //await paymentService.HandlePaymentSuccess(paymentId, bookingId);
        return Ok("Payment Successful");
    }

    [HttpPost("{paymentId}/cancel")]
    public async Task<IActionResult> PaymentCancel([FromRoute] Guid paymentId)
    {
        //await paymentService.HandlePaymentCancel(paymentId, bookingId);
        return Ok("Payment Cancelled");
    }
}