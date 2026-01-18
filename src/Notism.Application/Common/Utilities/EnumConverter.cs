namespace Notism.Application.Common.Utilities;

public static class EnumConverter
{
    /// <summary>
    /// Converts a string to an enum value using case-insensitive comparison.
    /// </summary>
    /// <typeparam name="TEnum">The enum type.</typeparam>
    /// <param name="value">The string value to convert.</param>
    /// <returns>The enum value if conversion is successful, null otherwise.</returns>
    public static TEnum? FromString<TEnum>(string? value)
        where TEnum : struct, Enum
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return Enum.TryParse<TEnum>(value, ignoreCase: true, out var result) ? result : null;
    }

    /// <summary>
    /// Converts an enum value to its camelCase string representation.
    /// </summary>
    /// <typeparam name="TEnum">The enum type.</typeparam>
    /// <param name="enumValue">The enum value to convert.</param>
    /// <returns>The camelCase string representation.</returns>
    public static string ToCamelCase<TEnum>(TEnum enumValue)
        where TEnum : struct, Enum
    {
        var enumString = enumValue.ToString();
        return char.ToLowerInvariant(enumString[0]) + enumString[1..];
    }

    /// <summary>
    /// Validates if a string can be converted to the specified enum type.
    /// </summary>
    /// <typeparam name="TEnum">The enum type.</typeparam>
    /// <param name="value">The string value to validate.</param>
    /// <returns>True if the string is valid for the enum type, false otherwise.</returns>
    public static bool IsValidEnumString<TEnum>(string? value)
        where TEnum : struct, Enum
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return true; // Allow null/empty as it might be optional
        }

        return Enum.TryParse<TEnum>(value, ignoreCase: true, out _);
    }

    /// <summary>
    /// Gets all valid string values for the specified enum type in camelCase.
    /// </summary>
    /// <typeparam name="TEnum">The enum type.</typeparam>
    /// <returns>An enumerable of valid camelCase string values.</returns>
    public static IEnumerable<string> GetValidEnumStrings<TEnum>()
        where TEnum : struct, Enum
    {
        return Enum.GetValues<TEnum>().Select(ToCamelCase);
    }
}