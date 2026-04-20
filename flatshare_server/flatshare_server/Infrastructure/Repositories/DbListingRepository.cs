using Microsoft.EntityFrameworkCore;
using flatshare_server.Infrastructure.Model.Listings;
using flatshare_server.Infrastructure.Model.Requests.Listing;

namespace flatshare_server.Infrastructure.Repositories;

public class DbListingRepository(FlatshareDbContext context) : IListingRepository
{
    public async Task<(List<Listing> Items, int TotalCount)> GetFilteredListingsAsync(ListingFilter filter, int page, int size)
    {
        var query = context.Listings
            .Include(l => l.Address)
            .Include(l => l.Attributes)
            .AsQueryable();

        if (!string.IsNullOrEmpty(filter.City))
            query = query.Where(l => l.Address.City == filter.City);

        if (filter.MinPrice.HasValue)
            query = query.Where(l => l.Price >= filter.MinPrice.Value);

        int totalCount = await query.CountAsync();
        var items = await query
            .Skip(page * size)
            .Take(size)
            .ToListAsync();

        return (items, totalCount);
    }
}