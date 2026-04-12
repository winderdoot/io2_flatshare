namespace flatshare_server.Infrastructure.Model.Requests;

public record TenantPreferencesDTO(
    decimal? MaxPrice,
    string? Currency,
    bool? SmokingAllowed,
    bool? PetsAllowed,
    List<string>? PreferredDistricts
);