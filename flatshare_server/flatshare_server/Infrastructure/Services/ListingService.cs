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

    /* Method is meant to return model entity, not DTO */ 
    public async Task<Listing> GetByIdAsync(Guid id)
    {
        var listing = await _context.Listings.FindAsync(id);
        if (listing is null)
        {
            throw ErrorResponse.Generate("Listing not found", StatusCodes.Status404NotFound);
        }
        return listing;
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

    public async Task<List<ListingDTO>> GetAllAsync()
    {
        var results = await _context.Listings.ToListAsync();
        return [.. results.Select(l => l.IntoDTO())];
    }

    public async Task<Listing> UpdateAsync(Guid id, UpdateListingRequest request)
    {
        var listing = await GetByIdAsync(id);
        listing.ApplyUpdate(request);

        await _context.SaveChangesAsync();
        return listing;
    }

    public async Task SubmitAsync(Guid id)
    {
        var listing = await GetByIdAsync(id);
        listing.SubmitForReview();
        await _context.SaveChangesAsync();
    }
    public async Task RequestFixesAsync(Guid id)
    {
        var listing = await GetByIdAsync(id);
        listing.RequestFixes();
        await _context.SaveChangesAsync();
    }
    public async Task ApproveAsync(Guid id)
    {
        var listing = await GetByIdAsync(id);
        listing.Approve();
        await _context.SaveChangesAsync();
    }
    public async Task HideAsync(Guid id)
    {
        var listing = await GetByIdAsync(id);
        listing.Hide();
        await _context.SaveChangesAsync();
    }
    public async Task PublishAsync(Guid id)
    {
        var listing = await GetByIdAsync(id);
        listing.Publish();
        await _context.SaveChangesAsync();
    }
    public async Task ArchiveAsync(Guid id)
    {
        var listing = await GetByIdAsync(id);
        listing.Archive();
        await _context.SaveChangesAsync();
    }
}
