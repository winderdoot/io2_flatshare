using System.Text;
using System.Security.Cryptography;

namespace flatshare_server.Infrastructure.Utils;

public static class Crypto
{
    public static string Sha256String(string input)
    {
        if (string.IsNullOrEmpty(input))
        {
            throw new ArgumentNullException(input);
        }

        using (SHA256 sha256 = SHA256.Create())
        {
            byte[] inputBytes = Encoding.UTF8.GetBytes(input);
            byte[] hashBytes = sha256.ComputeHash(inputBytes);
            return Convert.ToHexString(hashBytes).ToLowerInvariant();
        }
    }
}
