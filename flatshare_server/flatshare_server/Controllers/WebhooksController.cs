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
        string? sigHeader = Request.Headers["Stripe-Signature"].FirstOrDefault();
        if (sigHeader is null)
        {
            return BadRequest(new { Error = "Stripe-Signature header required" });
        }

        string payload = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
        await webhookService.HandleStripeWebhook(payload, sigHeader);

        /* Always return 200 to acknowledge that we received the hook to stripe */
        return Ok();
    }

}