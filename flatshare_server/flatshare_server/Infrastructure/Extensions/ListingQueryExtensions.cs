using flatshare_server.Infrastructure.Utils;
using flatshare_server.Infrastructure.Model.Listings;
using flatshare_server.Infrastructure.Model.Requests.Matches;

namespace flatshare_server.Infrastructure.Extensions;

public static class ListingQueryExtensions
{
    public static IQueryable<Listing> ApplyMatchesFilter(this IQueryable<Listing> query, MatchesFilter filter)
    {
        /* Only show Active listings */
        query = query.Where(l => l.Status == Listing.ListingStatus.Active);

        /* Apply Address Filters */
        if (!string.IsNullOrWhiteSpace(filter.City))
        {
            var cityEquivalents = CityNormalizer.GetEquivalents(filter.City).ToList();
            query = query.Where(l => cityEquivalents.Contains(l.Address.City));
        }

        if (!string.IsNullOrWhiteSpace(filter.District))
            query = query.Where(l => l.Address.District.ToLower() == filter.District.ToLower());

        /* Apply Price Filters
         * Interpret price in native listing currency */
        if (filter.MinPrice.HasValue)
        {
            query = query.Where(l => l.Price.Value >= filter.MinPrice.Value);
        }
        if (filter.MaxPrice.HasValue)
        {
            query = query.Where(l => l.Price.Value <= filter.MaxPrice.Value);
        }

        /* 4. Apply Area Filters */
        if (filter.MinArea.HasValue)
            query = query.Where(l => l.AreaMeterSq >= filter.MinArea.Value);
        if (filter.MaxArea.HasValue)
            query = query.Where(l => l.AreaMeterSq <= filter.MaxArea.Value);

        /* 5. Apply Date Filters */
        if (filter.StartDate.HasValue)
        {
            query = query.Where(l =>
                l.AvailableSince <= filter.StartDate.Value &&
                l.AvailableUntil >= filter.StartDate.Value);
        }

        /* 6. Apply Attributes */
        if (filter.PetsAllowed.HasValue)
            query = query.Where(l => l.Attributes.PetsAllowed == filter.PetsAllowed.Value);

        if (filter.NonSmokingOnly.HasValue)
            query = query.Where(l => l.Attributes.NonSmokingOnly == filter.NonSmokingOnly.Value);

        if (filter.CloseToShops.HasValue)
            query = query.Where(l => l.Attributes.CloseToShops == filter.CloseToShops.Value);

        /* Parse profile string to enum before querying */
        if (!string.IsNullOrWhiteSpace(filter.Profile) &&
            Enum.TryParse<ListingAttributes.TenantProfile>(filter.Profile, true, out var targetProfile))
        {
            query = query.Where(l => l.Attributes.Profile == targetProfile);
        }

        return query;
    }
}
