namespace flatshare_server.Infrastructure.Configuration;

public class EmailOptions
{
    public const string OptionsKey = "EmailOptions";
    public string AppName { get; set; }
    public string Host { get; set; }
    public int Port { get; set; }
    public string EmailAddress { get; set; }
    public string AppPassword { get; set; }
}
