using flatshare_server.Infrastructure.Model.Responses;
using flatshare_server.Infrastructure.Model.Users;
using flatshare_server.Infrastructure.Repositories;
using flatshare_server.Infrastructure.Services;
using flatshare_server.Infrastructure.Utils;
using Microsoft.AspNetCore.Mvc;

namespace flatshare_server.Controllers;

[ApiController]
[Route("api/v1/listings/{listingId}/photos")]
public class ListingPhotosController : Controller
{
    IStorageService _storage;
    ListingPhotoService _photos;
    ListingService _listings;
    FlatshareDbContext _dbContext;
    public ListingPhotosController
    (
        IStorageService storage,
        ListingService listings,
        ListingPhotoService photos,
        FlatshareDbContext dbContext
    )
    {
        _storage = storage;
        _listings = listings;
        _photos = photos;
        _dbContext = dbContext;
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById([FromRoute] Guid listingId, [FromRoute] Guid id)
    {
        var (stream, contentType) = await _storage.GetFileAsync(id);
        if (stream == null)
        {
            return NotFound();
        }

        return File(stream, contentType ?? "application/octet-stream");
    }
    [HttpGet]
    public async Task<ActionResult<ListingPhotosResponse>> GetAll([FromRoute] Guid listingId)
    {
        return await _photos.GetPhotosAsync(listingId);
    }

    [HttpPost]
    [RequestSizeLimit(32 * NumericConstants.Megabyte)]
    public async Task<IActionResult> CreateNew([FromRoute] Guid listingId, [FromForm] IFormFile file)
    {
        var fileId = await _photos.UploadPhotoAsync(listingId, file);

        return CreatedAtAction(
            actionName: nameof(GetById),
            routeValues: new { fileId },
            value: new { id = fileId }
        );
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete([FromRoute] Guid listingId, [FromRoute] Guid id)
    {
        var wasDeleted = await _photos.DeleteAsync(listingId, id);
        if (wasDeleted)
        {
            return NoContent();
        }
        return NotFound($"Photo '{id}' could not be found");
    }

}
