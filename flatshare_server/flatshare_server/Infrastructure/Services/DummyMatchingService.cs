using flatshare_server.Infrastructure.Repositories;
using flatshare_server.Infrastructure.Model.Responses;
using flatshare_server.Infrastructure.Model.Requests.Listing;

namespace flatshare_server.Infrastructure.Services;

public class DummyMatchingService(
    IListingRepository listingRepository,
    AuthService authService) : IMatchingService
{
    public async Task<PageResponse<MatchDTO>> GetMatchesAsync(Guid userId, ListingFilter filter, int page, int size)
    {
        var (listings, totalCount) = await listingRepository.GetFilteredListingsAsync(filter, page, size);

        var random = new Random();

        var matches = listings.Select(l =>
        {
            var baseDto = l.IntoDTO();

            return new MatchDTO
            {
                // Inicjalizacja pól z bazowego DTO
                Id = baseDto.Id,
                Title = baseDto.Title,
                Description = baseDto.Description,
                Price = baseDto.Price,
                Currency = baseDto.Currency,
                Area = baseDto.Area,
                AvailableSince = baseDto.AvailableSince,
                AvailableUntil = baseDto.AvailableUntil,
                OwnerContact = baseDto.OwnerContact,
                Location = baseDto.Location,
                Attributes = baseDto.Attributes,

                // Mock Algo
                MatchScore = Math.Round(random.NextDouble(), 2)
            };
        })
        .OrderByDescending(m => m.MatchScore)
        .ToList();

        return new PageResponse<MatchDTO>
        {
            Content = matches,
            Page = new PageMetadata
            {
                Size = size,
                Number = page,
                TotalElements = totalCount,
                TotalPages = (int)Math.Ceiling(totalCount / (double)size)
            }
        };
    }
}