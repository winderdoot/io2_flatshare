using flatshare_server.Infrastructure.Model.Responses;
using Microsoft.AspNetCore.Mvc;
using flatshare_server.Infrastructure.Model.Listings;
using flatshare_server.Infrastructure.Services;
using Azure.Storage.Blobs.Models;
using flatshare_server.Infrastructure.Model.Requests.Listing;
using Microsoft.AspNetCore.Authorization;

namespace flatshare_server.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class ListingsController : Controller
{
    private readonly ListingService _service;
    private readonly AuthService _auth;
    public ListingsController
    (
        ListingService listingService,
        AuthService authService
    ) 
    {
        _service = listingService;
        _auth = authService;
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ListingDTO>> Get([FromRoute] Guid id)
    {
        return Ok(await _service.GetByIdAsync(id));
    }
    [HttpGet]
    public async Task<ActionResult<ListingDTO>> Get([FromQuery] ListingFilter filter)
    {
        return Ok(_service.GetByFilterAsync(filter));
    }

    [HttpPost]
    [Authorize(Policy = AuthService.LandlordPolicy)]
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
    public async Task<ActionResult<ListingDTO>> Update([FromRoute] Guid id, [FromBody] UpdateListingRequest request)
    {
        throw new NotImplementedException();
    }

    /* RPC like Actions */ 
    [HttpPost("{id}/submit")]
    public async Task<IActionResult> Submit([FromRoute] Guid id)
    {
        throw new NotImplementedException();
    }

    [HttpPost("{id}/request-fixes")]
    public async Task<IActionResult> RequestFixes([FromRoute] Guid id)
    {
        throw new NotImplementedException();
    }
    [HttpPost("{id}/approve")]
    public async Task<IActionResult> Approve([FromRoute] Guid id)
    {
        throw new NotImplementedException();
    }
    [HttpPost("{id}/hide")]
    public async Task<IActionResult> Hide([FromRoute] Guid id)
    {
        throw new NotImplementedException();
    }
    [HttpPost("{id}/publish")]
    public async Task<IActionResult> Publish([FromRoute] Guid id)
    {
        throw new NotImplementedException();
    }
    [HttpPost("{id}/archive")]
    public async Task<IActionResult> Archive([FromRoute] Guid id)
    {
        throw new NotImplementedException();
    }
}
