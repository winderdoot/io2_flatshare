using flatshare_server.Infrastructure.Model.Listings;
using flatshare_server.Infrastructure.Model.Requests;
using flatshare_server.Infrastructure.Model.Responses;
using flatshare_server.Infrastructure.Repositories;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.EntityFrameworkCore;

namespace flatshare_server.Infrastructure.Services;

public class ListingService
{
    /* Avoid adding an additional ListingRepository because it's an unnecessary abstraction layer
     * that doesn't provide any benefits. */ 
    private FlatshareDbContext _context;
    public ListingService
    (
        FlatshareDbContext dbContext
    )
    {
        _context = dbContext;
    }

    public async Task<ListingCreatedResponse> CreateNewAsync(CreateListingRequest request)
    {
        var listing = Listing.TryCreate(request);
        _context.Listings.Add(listing);
        await _context.SaveChangesAsync();

        return new ListingCreatedResponse(listing.Id, listing.Status, listing.CreatedAt);
    }

    public async Task<ListingDTO> GetByIdAsync(Guid id)
    {
        var listing = await _context.Listings.FindAsync(id);
        if (listing is null)
        {
            throw ErrorResponse.Generate("Listing not found", StatusCodes.Status404NotFound);
        }
        return listing.IntoDTO();
    }
}
