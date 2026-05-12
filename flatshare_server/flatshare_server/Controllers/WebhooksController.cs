namespace flatshare_server.Controllers;

using flatshare_server.Infrastructure.Configuration;
using flatshare_server.Infrastructure.Services.Bookings;
using Microsoft.AspNetCore.Mvc;
using Stripe;
using Stripe.Checkout;
using System.IO;

[ApiController]
[Route("api/v1/[controller]")]
public class WebhooksController 
(
    WebhookService webhookService    
)
    : ControllerBase
{
    [HttpPost("init-secret")]
    public IActionResult InitSecret([FromBody] WebhookInitRequest request)
    {
        string? initKey = Request.Headers["X-Init-Key"].FirstOrDefault();
        if (initKey is null)
        {
            return BadRequest(new { Error = "X-Init-Key header required" });
        }
        webhookService.InitializeSecret(request.Secret, initKey);

        return Ok();
    }

    [HttpPost]
    public async Task<IActionResult> HandleStripeWebhook()
    {
        await Task.CompletedTask;
        return Ok();
        //var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();

        //try
        //{
        //    // This verifies the request actually came from Stripe
        //    var stripeEvent = EventUtility.ConstructEvent(
        //        json,
        //        Request.Headers["Stripe-Signature"],
        //        _webhookSecret
        //    );

        //    // Handle the successful payment
        //    if (stripeEvent.Type == Events.CheckoutSessionCompleted)
        //    {
        //        var session = stripeEvent.Data.Object as Session;

        //        // Retrieve the BookingId we attached earlier
        //        var bookingId = Guid.Parse(session.ClientReferenceId);

        //        // TODO: Fetch booking & payment from your Database
        //        // var booking = db.Bookings.Find(bookingId);

        //        // Execute your state machine logic!
        //        // payment.GatewayConfirmed();
        //        // booking.PaymentSuccess();
        //        // await db.SaveChangesAsync();

        //        Console.WriteLine($"Payment successful for Booking: {bookingId}");
        //    }
        //    // Handle expired/cancelled sessions
        //    else if (stripeEvent.Type == Events.CheckoutSessionExpired)
        //    {
        //        var session = stripeEvent.Data.Object as Session;
        //        var bookingId = Guid.Parse(session.ClientReferenceId);

        //        // Execute your cancellation state machine logic
        //        // payment.UserAborted();
        //        // booking.PaymentTimeout();
        //        Console.WriteLine($"Payment expired for Booking: {bookingId}");
        //    }

        //    // Always return a 200 OK to Stripe so they know you received it
        //    return Ok();
        //}
        //catch (StripeException e)
        //{
        //    return BadRequest(new { Error = e.Message });
        //}
    }
}