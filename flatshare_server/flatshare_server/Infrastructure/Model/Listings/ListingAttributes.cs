namespace flatshare_server.Infrastructure.Model.Listings;

public record class ListingAttributes
{
    public enum TenantProfile
    {
        Student,
        Tourist
    }

    public bool PetsAllowed { get; init; } = true;
    public bool NonSmokingOnly { get; init; } = false;
    public bool CloseToShops { get; init; } = true;
    public required TenantProfile Profile { get; init; }
}
