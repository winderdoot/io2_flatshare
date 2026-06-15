namespace flatshare_server.Infrastructure.Model.Listings;

public record class Unavailability
{
    public required DateOnly Since { get; init; }
    public required DateOnly Until { get; init; }
    public required string Message { get; init; }
}

public record class UnavailabilityRange
{
    public required DateOnly Since { get; init; }
    public required DateOnly Until { get; init; }
}
