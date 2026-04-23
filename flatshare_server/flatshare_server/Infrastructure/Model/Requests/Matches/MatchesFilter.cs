namespace flatshare_server.Infrastructure.Model.Requests.Matches;

public record MatchesFilter(
    int Page = 0,
    int Size = 10,
    string? City = null,
    string? District = null,
    decimal? MinPrice = null,
    decimal? MaxPrice = null,
    bool? PetsAllowed = null,
    bool? NonSmokingOnly = null,
    bool? CloseToShops = null,
    string? Profile = null,
    double? MinArea = null,
    double? MaxArea = null,
    DateOnly? StartDate = null
);