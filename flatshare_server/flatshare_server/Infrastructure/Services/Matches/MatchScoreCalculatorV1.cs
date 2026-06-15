using flatshare_server.Infrastructure.Model;
using flatshare_server.Infrastructure.Model.Listings;
using flatshare_server.Infrastructure.Model.Requests.Matches;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace flatshare_server.Infrastructure.Services.Listings;

public class MatchScoreCalculatorV1 : IMatchScoreCalculator
{
    public double Score(Listing listing, MatchesFilter filter)
    {
        double score = 0.0;

        /* Everything is accounted for on the db side (filtering), so we just give a score based on price and area */
        decimal priceInPLN = listing.Price.ApproxPLNValue();
        double priceScore = 5.0 - 0.4 * Math.Pow((double)priceInPLN, 0.3);
        score += Math.Max(0, priceScore);

        double areaScore = 0.005 * Math.Pow((double)listing.AreaMeterSq, 1.5);
        score += Math.Min(areaScore, 3.0);

        /* To make the algorithm look smarter, we increase score if filter is more specific */
        if (filter.PetsAllowed.HasValue)
        {
            score += 0.8;
        }
        if (filter.NonSmokingOnly.HasValue)
        {
            score += 0.5;
        }
        if (filter.CloseToShops.HasValue)
        {
            score += 0.6;
        }
        if (!string.IsNullOrEmpty(filter.Profile))
        {
            score += 1.0;
        }

        if (filter.StartDate.HasValue)
        {
            var daysAvailable = (listing.AvailableUntil.ToDateTime(TimeOnly.MinValue) - filter.StartDate.Value.ToDateTime(TimeOnly.MinValue)).TotalDays;
            daysAvailable = Math.Min(daysAvailable, 2500);
            if (daysAvailable >= 0)
            {
                score += -1.0 + 0.005 * Math.Pow(daysAvailable, 1.2) - 0.001 * Math.Pow(daysAvailable, 1.4);
            }
            else
            {
                /* Penality */
                score += -1.0 - 0.005 * Math.Pow(Math.Abs(daysAvailable), 1.2) + 0.001 * Math.Pow(Math.Abs(daysAvailable), 1.4);
            }
        }

        return score;
    }
}
