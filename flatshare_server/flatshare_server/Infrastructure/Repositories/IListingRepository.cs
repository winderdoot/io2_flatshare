using flatshare_server.Infrastructure.Model.Listings;
using flatshare_server.Infrastructure.Model.Requests.Listing;

namespace flatshare_server.Infrastructure.Repositories;

public interface IListingRepository
{
    Task<(List<Listing> Items, int TotalCount)> GetFilteredListingsAsync(ListingFilter filter, int page, int size);
}