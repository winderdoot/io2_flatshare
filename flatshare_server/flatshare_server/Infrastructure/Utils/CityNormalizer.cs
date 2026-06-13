namespace flatshare_server.Infrastructure.Utils;

public static class CityNormalizer
{
    private static readonly Dictionary<string, HashSet<string>> EquivalentGroups =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ["warszawa"] = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "Warszawa",
                "Warsaw",
            },
            ["krakow"] = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "Kraków",
                "Krakow",
                "Cracow",
            },
            ["wroclaw"] = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "Wrocław",
                "Wroclaw",
            },
        };

    public static HashSet<string> GetEquivalents(string city)
    {
        if (string.IsNullOrWhiteSpace(city))
        {
            return new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        }

        foreach (var group in EquivalentGroups.Values)
        {
            if (group.Contains(city))
            {
                return group;
            }
        }

        return new HashSet<string>(StringComparer.OrdinalIgnoreCase) { city.Trim() };
    }

    public static bool Matches(string listingCity, string filterCity)
    {
        if (string.IsNullOrWhiteSpace(filterCity))
        {
            return true;
        }

        var equivalents = GetEquivalents(filterCity);
        return equivalents.Contains(listingCity);
    }
}
