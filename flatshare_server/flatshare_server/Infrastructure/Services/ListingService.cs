using Azure.Core;
using flatshare_server.Infrastructure.Model.Listings;
using flatshare_server.Infrastructure.Model.Requests.Listing;
using flatshare_server.Infrastructure.Model.Responses;
using flatshare_server.Infrastructure.Repositories;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.EntityFrameworkCore;
using System.Security.Principal;

namespace flatshare_server.Infrastructure.Services;

public class ListingService
{
    /* Avoid adding an additional ListingRepository because it's an unnecessary abstraction layer
     * that doesn't provide any benefits. */ 
    private FlatshareDbContext _context;
    private UserService _userService;
    public ListingService
    (
        FlatshareDbContext dbContext,
        UserService userService
    )
    {
        _context = dbContext;
        _userService = userService;
    }

    public async Task<ListingCreatedResponse> CreateNewAsync(CreateListingRequest request, Guid ownerId)
    {
        var owner = await _userService.GetByIdAsync(ownerId);

        var listing = Listing.TryCreate(request, owner);
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

    public async Task<List<ListingDTO>> GetByFilterAsync(ListingFilter filter)
    {
        var query = _context.Listings.AsQueryable();

        if (filter.OwnerId.HasValue)
        {
            query = query.Where(l => EF.Property<Guid>(l, "OwnerId") == filter.OwnerId.Value);
        }

        if (!string.IsNullOrWhiteSpace(filter.City))
        {
            query = query.Where(l => l.Address.City == filter.City);

            if (!string.IsNullOrWhiteSpace(filter.District))
            {
                query = query.Where(l => l.Address.District == filter.District);

                if (!string.IsNullOrWhiteSpace(filter.Street))
                {
                    query = query.Where(l => l.Address.Street == filter.Street);

                    if (!string.IsNullOrWhiteSpace(filter.AptNumber))
                    {
                        query = query.Where(l => l.Address.AptNumber == filter.AptNumber);
                    }
                }
            }
        }

        var results = await query.ToListAsync();
        return [.. results.Select(listing => listing.IntoDTO())];
    }
}
