using System.Text.Json.Serialization;

namespace flatshare_server.Infrastructure.Model.Responses;

public record class MatchDTO
{
    public required ListingDTO Listing { get; init; }
    public required double MatchScore { get; init; }
}