using flatshare_server.Infrastructure.Model.Requests;

namespace flatshare_server.Infrastructure.Model.Users;

public class TenantPreferences
{
    public decimal? MaxPrice { get; private set; }
    public string? Currency { get; private set; }
    public bool? SmokingAllowed { get; private set; }
    public bool? PetsAllowed { get; private set; }
    public List<string> PreferredDistricts { get; private set; } = new();

    public void UpdatePreferences(TenantPreferencesDTO dto)
    {
        MaxPrice = dto.MaxPrice;
        Currency = dto.Currency;
        SmokingAllowed = dto.SmokingAllowed;
        PetsAllowed = dto.PetsAllowed;
        PreferredDistricts = dto.PreferredDistricts ?? new List<string>();
    }

    public TenantPreferencesDTO IntoDTO()
    {
        return new TenantPreferencesDTO(
            MaxPrice,
            Currency,
            SmokingAllowed,
            PetsAllowed,
            PreferredDistricts
        );
    }
}