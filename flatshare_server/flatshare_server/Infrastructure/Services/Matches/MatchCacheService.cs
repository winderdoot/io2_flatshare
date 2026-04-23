using flatshare_server.Infrastructure.Extensions;
using flatshare_server.Infrastructure.Model.Listings;
using flatshare_server.Infrastructure.Model.Requests.Matches;
using flatshare_server.Infrastructure.Repositories;
using flatshare_server.Infrastructure.Services.Listings;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System.Text.Json;

namespace flatshare_server.Infrastructure.Services.Matches;

public record ScoredListing(
    Listing Listing,
    double Score
);

public class MatchCacheService
    (
        IMemoryCache cache,
        FlatshareDbContext dbContext,
        IMatchScoreCalculator matchCalculator
    )
{
    public async Task<List<ScoredListing>> GetListingsAsync(MatchesFilter filter, Guid userId)
    {
        string cacheKey = GenerateCacheKey(filter, userId);

        var fullList = await cache.GetOrCreateAsync(cacheKey, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10);
            entry.SlidingExpiration = TimeSpan.FromMinutes(2);

            return await FetchMatchesAsync(filter);
        });

        return fullList!;
    }

    private string GenerateCacheKey(MatchesFilter filter, Guid userId)
    {
        var cacheFilter = filter with { Page = 0, Size = 0 };

        return $"matches_user:{userId}_filter:{JsonSerializer.Serialize(cacheFilter)}";
    }

    private async Task<List<ScoredListing>> FetchMatchesAsync(MatchesFilter filter)
    {
        var baseQuery = dbContext.Listings.AsNoTracking();
        var filteredQuery = baseQuery.ApplyMatchesFilter(filter);
        var listings = await filteredQuery.ToListAsync();

        var scoredListings = listings
            .Select(listing => new ScoredListing(
                listing, 
                matchCalculator.Score(listing, filter)
            ))
            .OrderByDescending(x => x.Score)
            .ToList();

        return scoredListings;
    }
}

