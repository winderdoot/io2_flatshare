using flatshare_server.Infrastructure.Model.Requests;
using flatshare_server.Infrastructure.Model.Responses;
using Microsoft.AspNetCore.Mvc;
using flatshare_server.Infrastructure.Model.Listings;

namespace flatshare_server.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class ListingsController : Controller
{
    public ListingsController() { }

    [HttpGet("{id}")]
    public async Task<ActionResult<ListingDTO>> Get([FromRoute] Guid id)
    {
        throw new NotImplementedException();
    }

    [HttpPost]
    public async Task<ActionResult<ListingCreatedResponse>> CreateNew([FromBody] CreateListingRequest request)
    {
        throw new NotImplementedException();
    }

    [HttpPatch("{id}")]
    public async Task<ActionResult<ListingDTO>> Update([FromRoute] Guid id, [FromBody] UpdateListingRequest request)
    {
        throw new NotImplementedException();
    }

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
