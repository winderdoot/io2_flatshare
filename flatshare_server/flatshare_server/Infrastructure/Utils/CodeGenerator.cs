using System.Security.Cryptography;
namespace flatshare_server.Infrastructure.Utils;

public static class CodeGenerator
{
    private const string Alphanumeric = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

    public static string GenerateResetCode(int length = 10)
    {
        return string.Create(length, Alphanumeric, (chars, possibleChars) =>
        {
            for (int i = 0; i < chars.Length; i++)
            {
                chars[i] = possibleChars[RandomNumberGenerator.GetInt32(possibleChars.Length)];
            }
        });
    }
}
