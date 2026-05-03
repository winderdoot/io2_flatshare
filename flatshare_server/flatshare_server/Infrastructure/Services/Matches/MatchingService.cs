using flatshare_server.Infrastructure.Extensions;
using flatshare_server.Infrastructure.Model.Listings;
using flatshare_server.Infrastructure.Model.Requests;
using flatshare_server.Infrastructure.Model.Requests.Listing;
using flatshare_server.Infrastructure.Model.Requests.Matches;
using flatshare_server.Infrastructure.Model.Responses;
using flatshare_server.Infrastructure.Repositories;
using flatshare_server.Infrastructure.Services.Listings;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System.Linq;
using System.Text.Json;

namespace flatshare_server.Infrastructure.Services;

public record ScoredListing(
    Listing Listing,
    double Score
);

public class MatchingService
    (
        UserService userService,
        IMemoryCache cache,
        FlatshareDbContext dbContext,
        IMatchScoreCalculator matchCalculator
    )
{
    public async Task<List<ScoredListing>> GetCachedListingsAsync(MatchesFilter filter, Guid userId)
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
    private MatchesFilter ApplyPreferences(TenantPreferencesDTO preferences, MatchesFilter filter)
    {
        return filter with
        {
            MaxPrice = preferences.MaxPrice ?? filter.MaxPrice,
            PetsAllowed = preferences.PetsAllowed ?? filter.PetsAllowed,
            /* The attributes NonSmokingOnly and SmokingAllowed don't mean the same thing so we approximate a translation */
            NonSmokingOnly = (preferences.SmokingAllowed == true) ? false : filter.NonSmokingOnly
        };
    }
    public async Task<PageResponse<MatchDTO>> GetMatchesAsync(Guid userId, MatchesFilter filter)
    {
        var userPreferences = await userService.GetPreferencesAsync(userId);
        filter = ApplyPreferences(userPreferences, filter);

        /* Acquire cached results - if not present, they're created */
        var scoredListings = await GetCachedListingsAsync(filter, userId);

        var pagedData = scoredListings
            .Skip(filter.Page * filter.Size)
            .Take(filter.Size)
            .Select(x => new MatchDTO
            { 
                Listing = x.Listing.IntoDTO(),
                MatchScore = x.Score
            })
            .ToList();

        return new PageResponse<MatchDTO>
        {
            Content = pagedData,
            Page = new PageMetadata
            {
                Size = filter.Size,
                Number = filter.Page,
                TotalElements = scoredListings.Count,
                TotalPages = (int)Math.Ceiling(scoredListings.Count / (double)filter.Size)
            }
        };
    }
}
