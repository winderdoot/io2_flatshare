namespace flatshare_server.Infrastructure.Configuration;

public class JwtOptions
{
    public const string OptionsKey = "JwtOptions";
    public string Secret { get; set; }
    public string Issuer { get; set; }
    public string Audience { get; set; }
    public int ExpirationTimeInMinutes { get; set; }
}
