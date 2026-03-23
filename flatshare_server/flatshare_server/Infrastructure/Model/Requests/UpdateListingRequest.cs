namespace flatshare_server.Infrastructure.Model.Requests;
using flatshare_server.Infrastructure.Model.Listings;

public record class UpdateListingRequest
{
    public string? Title { get; init; } = null;
    public string? Description { get; init; } = null;
    public decimal? Price { get; init; } = null;
    public string? Currency { get; init; } = null;
    public DateOnly? AvailableSince { get; init; } = null;
    public DateOnly? AvailableUntil { get; init; } = null;
    public string? OwnerContact { get; init; } = null;
    public float? Area { get; init; } = null;
    public Address? Location { get; init; } = null;
    public ListingAttributes? Attributes { get; init; } = null;
}
