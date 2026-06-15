namespace flatshare_server.Infrastructure.Model.Responses;

using flatshare_server.Infrastructure.Model.Listings;
public record ListingCreatedResponse(Guid Id, Listing.ListingStatus Status, DateTime CreatedAt);

