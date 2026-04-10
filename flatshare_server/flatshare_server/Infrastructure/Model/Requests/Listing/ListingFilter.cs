namespace flatshare_server.Infrastructure.Model.Requests.Listing;

public record class ListingFilter
{
    public string? City { get; set; } = null;
    public string? District { get; set; } = null;
    public string? Street { get; set; } = null;
    public string? AptNumber { get; set; } = null;
    public Guid? OwnerId { get; set; } = null;
}
