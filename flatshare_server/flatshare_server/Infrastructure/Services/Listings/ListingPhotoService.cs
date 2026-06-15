using flatshare_server.Infrastructure.Model.Listings;
using flatshare_server.Infrastructure.Model.Requests;
using flatshare_server.Infrastructure.Model.Responses;
using flatshare_server.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace flatshare_server.Infrastructure.Services;

public class ListingPhotoService
{
    /* Avoid adding an additional ListingRepository because it's an unnecessary abstraction layer
     * that doesn't provide any benefits. */
    private FlatshareDbContext _context;
    private IStorageService _storage;
    private ListingService _listings;
    public ListingPhotoService
    (
        FlatshareDbContext dbContext,
        IStorageService storage,
        ListingService listingService
    )
    {
        _context = dbContext;
        _storage = storage;
        _listings = listingService;
    }

    public async Task<bool> DeleteAsync(Guid listingId, Guid id)
    {
        var listing = await _context.Listings.FindAsync(listingId);
        if (listing is null)
        {
            throw ErrorResponse.Generate("Listing not found", StatusCodes.Status404NotFound);
        }
        if (!listing.Photos.Remove(id))
        {
            throw ErrorResponse.Generate("Photo not found", StatusCodes.Status404NotFound);
        }
        await _context.SaveChangesAsync();

        return await _storage.DeleteFileAsync(id);
    }

    public async Task<ListingPhotosResponse> GetPhotosAsync(Guid listingId)
    {
        var listing = await _context.Listings.FindAsync(listingId);
        if (listing is null)
        {
            throw ErrorResponse.Generate("Listing not found", StatusCodes.Status404NotFound);
        }
        return new ListingPhotosResponse(listingId, listing.Photos);
    }

    public async Task<Guid> UploadPhotoAsync(Guid listingId, IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            throw ErrorResponse.Generate("No photo provided", StatusCodes.Status400BadRequest);
        }

        var listing = await _context.Listings.FindAsync(listingId);
        if (listing is null)
        {
            throw ErrorResponse.Generate("Listing not found", StatusCodes.Status400BadRequest);
        }

        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!allowedExtensions.Contains(extension))
        {
            throw ErrorResponse.Generate("Invalid file type", StatusCodes.Status400BadRequest);
        }

        try
        {
            Guid fileId = await _storage.StoreFileAsync(file);
            listing.Photos.Add(fileId);
            await _context.SaveChangesAsync();

            return fileId;
        }
        catch (Exception ex)
        {
            throw ErrorResponse.Generate($"Unexpected error: {ex.Message}", StatusCodes.Status400BadRequest);
        }
    }
}