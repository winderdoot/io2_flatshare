using flatshare_server.Infrastructure.Model.Responses;
using flatshare_server.Infrastructure.Model.Requests.Listing;

namespace flatshare_server.Infrastructure.Services;

public interface IMatchingService
{
    Task<PageResponse<MatchDTO>> GetMatchesAsync(Guid userId, ListingFilter filter, int page, int size);
}