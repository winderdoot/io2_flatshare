namespace flatshare_server.Infrastructure.Model.Requests;
using flatshare_server.Infrastructure.Model.Listings;

public record class CreateListingRequest
{
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
}
