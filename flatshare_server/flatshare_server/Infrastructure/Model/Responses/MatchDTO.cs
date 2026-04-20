using System.Text.Json.Serialization;

namespace flatshare_server.Infrastructure.Model.Responses;

public class MatchDTO : ListingDTO
{
    // Ignorujemy standardowe "id" z klasy bazowej, ¿eby nie dublowaæ pól w JSON
    [JsonIgnore]
    public new Guid Id { get => base.Id; init => base.Id = value; }

    [JsonPropertyName("listingId")]
    public Guid ListingId { get => base.Id; init => base.Id = value; }

    public double MatchScore { get; init; }
}