using flatshare_server.Infrastructure.Model.Responses;
using Microsoft.AspNetCore.Mvc;
using flatshare_server.Infrastructure.Model.Listings;
using flatshare_server.Infrastructure.Services;
using Azure.Storage.Blobs.Models;
using flatshare_server.Infrastructure.Model.Requests.Listing;
using Microsoft.AspNetCore.Authorization;
using Org.BouncyCastle.Asn1.BC;

namespace flatshare_server.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class ListingsController
(
    ListingService listingService,
    BookingService bookingService
) : Controller
{
    private async Task AssertListingOwner(Guid listingId)
    {
        var listing = await listingService.GetByIdAsync(listingId, attachOwner: true);
        if (listing is null)
        {
            throw ErrorResponse.Generate("Listing not found", StatusCodes.Status404NotFound);
        }
        var ownerId = listing.Owner?.Id;
        if (ownerId is null)
        {
            throw ErrorResponse.Generate("Listing has no owner", StatusCodes.Status500InternalServerError);
        }
        if (ownerId != AuthService.GetUserId(User))
        {
            throw ErrorResponse.Generate("Unauthorized", StatusCodes.Status401Unauthorized);
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ListingDTO>> Get([FromRoute] Guid id)
    {
        return Ok((await listingService.GetByIdAsync(id)).IntoDTO());
    }
    [HttpGet]
    public async Task<ActionResult<ListingDTO>> Get([FromQuery] ListingFilter filter)
    {
        return Ok(await listingService.GetByFilterAsync(filter));
    }

    [HttpPost]
    [Authorize(Roles = AuthService.LandlordRole)]
    public async Task<ActionResult<ListingCreatedResponse>> CreateNew([FromBody] CreateListingRequest request)
    {
        Guid ownerId = AuthService.GetUserId(User);

        var listing = await listingService.CreateNewAsync(request, ownerId);
        return CreatedAtAction(
            actionName: nameof(Get),
            routeValues: new { id = listing.Id },
            value: listing
        );
    }

    [HttpPatch("{id}")]
    [Authorize(Roles = AuthService.LandlordRole)]
    public async Task<ActionResult<ListingDTO>> Update([FromRoute] Guid id, [FromBody] UpdateListingRequest request)
    {
        await AssertListingOwner(id);
        var updated = await listingService.UpdateAsync(id, request);
        return Ok(updated.IntoDTO());
    }

    [HttpPatch("{id}/submit")]
    [Authorize(Roles = AuthService.LandlordRole)]
    public async Task<IActionResult> Submit([FromRoute] Guid id)
    {
        await AssertListingOwner(id);
        await listingService.SubmitAsync(id);
        return NoContent();
    }

    [HttpPatch("{id}/request-fixes")]
    [Authorize(Roles = AuthService.AdminRole)]
    public async Task<IActionResult> RequestFixes([FromRoute] Guid id)
    {
        await listingService.RequestFixesAsync(id);
        return NoContent();
    }

    [HttpPatch("{id}/approve")]
    [Authorize(Roles = AuthService.AdminRole)]
    public async Task<IActionResult> Approve([FromRoute] Guid id)
    {
        await listingService.ApproveAsync(id);
        return NoContent();
    }

    [HttpPatch("{id}/hide")]
    [Authorize(Roles = AuthService.LandlordRole)]
    public async Task<IActionResult> Hide([FromRoute] Guid id)
    {
        await AssertListingOwner(id);
        await listingService.HideAsync(id);
        return NoContent();
    }

    [HttpPatch("{id}/publish")]
    [Authorize(Roles = AuthService.LandlordRole)]
    public async Task<IActionResult> Publish([FromRoute] Guid id)
    {
        await AssertListingOwner(id);
        await listingService.PublishAsync(id);
        return NoContent();
    }

    [HttpPatch("{id}/archive")]
    [Authorize(Roles = $"{AuthService.LandlordRole},{AuthService.AdminRole}")]
    public async Task<IActionResult> Archive([FromRoute] Guid id)
    {
        if (User.IsInRole(AuthService.LandlordRole))
        {
            await AssertListingOwner(id);
        }
        await listingService.ArchiveAsync(id);
        return NoContent();
    }

    [HttpPost("{id}/unavailability")]
    [Authorize(Roles = $"{AuthService.LandlordRole}")]
    public async Task<IActionResult> AddUnavailability([FromRoute] Guid id, [FromBody] Unavailability unavailability)
    {
        await AssertListingOwner(id);
        await listingService.AddUnavailabilityAsync(id, unavailability);
        return NoContent();
    }

    [HttpDelete("{id}/unavailability")]
    [Authorize(Roles = $"{AuthService.LandlordRole}")]
    public async Task<IActionResult> RemoveUnavailability([FromRoute] Guid id, [FromBody] UnavailabilityRange unavailability)
    {
        await AssertListingOwner(id);
        await bookingService.VerifyUnavailabilityCollisionsAsync(id, unavailability.Since, unavailability.Until);
        await listingService.RemoveUnavailabilityAsync(id, unavailability);
        return NoContent();
    }
    [HttpGet("under-review")]
    [Authorize(Roles = AuthService.AdminRole)]
    public async Task<ActionResult<List<ListingDTO>>> GetUnderReviewListings()
    {
        var listings = await listingService.GetListingsUnderReviewAsync();
        return Ok(listings);
    }

    [HttpPatch("{id}/moderation-hide")]
    [Authorize(Roles = AuthService.AdminRole)]
    public async Task<IActionResult> ModerationHide([FromRoute] Guid id)
    {
        await listingService.HideByModerationAsync(id);
        return NoContent();
    }

    [HttpPatch("{id}/reinstate")]
    [Authorize(Roles = AuthService.AdminRole)]
    public async Task<IActionResult> Reinstate([FromRoute] Guid id)
    {
        await listingService.ReinstateAsync(id);
        return NoContent();
    }
}
