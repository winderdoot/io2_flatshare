using flatshare_server.Infrastructure.Model.Listings;
using flatshare_server.Infrastructure.Model.Requests.Matches;

namespace flatshare_server.Infrastructure.Services.Listings;

public interface IMatchScoreCalculator
{
    public double Score(Listing listing, MatchesFilter filter);
}
