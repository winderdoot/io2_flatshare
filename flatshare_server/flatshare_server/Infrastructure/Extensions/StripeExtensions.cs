using Stripe.Checkout;

namespace flatshare_server.Infrastructure.Extensions;

public static class StripeExtensions
{
    /// <summary>
    /// Safely extracts a string from Stripe Metadata. Returns null if missing.
    /// </summary>
    public static string? GetMetadata(this Session session, string key)
    {
        if (session?.Metadata == null)
            return null;

        return session.Metadata.TryGetValue(key, out string? value) ? value : null;
    }

    /// <summary>
    /// Safely extracts and converts Metadata to a specific type (like Guid or int).
    /// </summary>
    public static T? GetMetadataAs<T>(this Session session, string key) where T : struct
    {
        var rawValue = session.GetMetadata(key);
        if (string.IsNullOrWhiteSpace(rawValue))
            return null;

        try
        {
            // Handles Guids natively
            if (typeof(T) == typeof(Guid))
                return (T)(object)Guid.Parse(rawValue);

            // Handles standard types (int, bool, double, etc.)
            return (T)Convert.ChangeType(rawValue, typeof(T));
        }
        catch
        {
            // If parsing fails, fail gracefully rather than crashing the webhook
            return null;
        }
    }
}