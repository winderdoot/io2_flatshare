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
public class ListingsController : Controller
{
    private readonly ListingService _service;
    private readonly AuthService _auth;
    private readonly UserService _users;
    public ListingsController
    (
        ListingService listingService,
        AuthService authService,
        UserService userService
    ) 
    {
        _service = listingService;
        _auth = authService;
        _users = userService;
    }

    private async Task AssertListingOwner(Guid listingId)
    {
        var listing = await _service.GetByIdAsync(listingId);
        if (listing is null)
        {
            throw ErrorResponse.Generate("Listing not found", StatusCodes.Status404NotFound);
        }
        var ownerId = listing.Owner?.Id;
        if (ownerId is null)
        {
            throw ErrorResponse.Generate("Listing has no owner", StatusCodes.Status500InternalServerError);
        }
        if (ownerId != _auth.GetUserId(User))
        {
            throw ErrorResponse.Generate("Unauthorized", StatusCodes.Status401Unauthorized);
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ListingDTO>> Get([FromRoute] Guid id)
    {
        return Ok((await _service.GetByIdAsync(id)).IntoDTO());
    }
    [HttpGet]
    public async Task<ActionResult<ListingDTO>> Get([FromQuery] ListingFilter filter)
    {
        return Ok(await _service.GetByFilterAsync(filter));
    }

    [HttpPost]
    [Authorize(Roles = AuthService.LandlordRole)]
    public async Task<ActionResult<ListingCreatedResponse>> CreateNew([FromBody] CreateListingRequest request)
    {
        Guid ownerId = _auth.GetUserId(User);

        var listing = await _service.CreateNewAsync(request, ownerId);
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
        var updated = await _service.UpdateAsync(id, request);
        return Ok(updated.IntoDTO());
    }

    [HttpPatch("{id}/submit")]
    [Authorize(Roles = AuthService.LandlordRole)]
    public async Task<IActionResult> Submit([FromRoute] Guid id)
    {
        await AssertListingOwner(id);
        await _service.SubmitAsync(id);
        return NoContent();
    }

    [HttpPatch("{id}/request-fixes")]
    [Authorize(Roles = AuthService.AdminRole)]
    public async Task<IActionResult> RequestFixes([FromRoute] Guid id)
    {
        await _service.RequestFixesAsync(id);
        return NoContent();
    }

    [HttpPatch("{id}/approve")]
    [Authorize(Roles = AuthService.AdminRole)]
    public async Task<IActionResult> Approve([FromRoute] Guid id)
    {
        await _service.ApproveAsync(id);
        return NoContent();
    }

    [HttpPatch("{id}/hide")]
    [Authorize(Roles = AuthService.LandlordRole)]
    public async Task<IActionResult> Hide([FromRoute] Guid id)
    {
        await AssertListingOwner(id);
        await _service.HideAsync(id);
        return NoContent();
    }

    [HttpPatch("{id}/publish")]
    [Authorize(Roles = AuthService.LandlordRole)]
    public async Task<IActionResult> Publish([FromRoute] Guid id)
    {
        await AssertListingOwner(id);
        await _service.PublishAsync(id);
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
        await _service.ArchiveAsync(id);
        return NoContent();
    }
}
