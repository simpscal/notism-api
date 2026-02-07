using System.Security.Cryptography;
using System.Text;

namespace Notism.Shared.Utilities;

public static class SlugGenerator
{
    private const string Characters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
    private const int DefaultLength = 8;

    /// <summary>
    /// Generates a friendly slug ID with an optional prefix.
    /// Format: [PREFIX-]XXXXXXXX (e.g., ORD-A1B2C3D4 or A1B2C3D4).
    /// </summary>
    /// <param name="prefix">Optional prefix for the slug (e.g., "ORD"). If null or empty, no prefix is added.</param>
    /// <param name="length">Length of the random string. Default is 8.</param>
    /// <returns>A friendly slug ID string.</returns>
    public static string Generate(string? prefix = null, int length = DefaultLength)
    {
        var randomString = GenerateRandomString(length);
        return string.IsNullOrWhiteSpace(prefix) ? randomString : $"{prefix}-{randomString}";
    }

    /// <summary>
    /// Generates a random alphanumeric string of the specified length.
    /// </summary>
    private static string GenerateRandomString(int length)
    {
        var result = new StringBuilder(length);
        var randomBytes = new byte[length];

        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(randomBytes);
        }

        for (int i = 0; i < length; i++)
        {
            result.Append(Characters[randomBytes[i] % Characters.Length]);
        }

        return result.ToString();
    }
}

