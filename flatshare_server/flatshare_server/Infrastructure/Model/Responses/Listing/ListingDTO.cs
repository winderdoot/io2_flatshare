using flatshare_server.Infrastructure.Model.Listings;

namespace flatshare_server.Infrastructure.Model.Responses;

public class ListingDTO
{
    public required Guid Id { get; set; }
    public required Listing.ListingStatus Status { get; init; }
    public required string Title { get; init; }
    public required string Description { get; init; }
    public required decimal Price { get; init; }
    public required string Currency { get; init; }
    public required DateOnly AvailableSince { get; init; }
    public required DateOnly AvailableUntil { get; init; }
    public required string OwnerContact { get; init; }
    public required float Area { get; init; }
    public required Address Location { get; init; }
    public required ListingAttributes Attributes { get; init; }
    public required List<Unavailability> Unavailabilities { get; init; }
}


