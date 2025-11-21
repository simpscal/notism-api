namespace Notism.Shared.Extensions;

public static class StringExtensions
{
    public static string UpperCaseFirstChar(this string input)
    {
        if (string.IsNullOrEmpty(input))
        {
            return input;
        }

        return $"{char.ToUpper(input[0])}{input.Substring(1)}";
    }

    /// <summary>
    /// Checks if the string is a valid absolute URL (http:// or https://).
    /// </summary>
    /// <param name="input">The string to check.</param>
    /// <returns>True if the string is a valid absolute URL, false otherwise.</returns>
    public static bool IsValidUrl(this string? input)
    {
        return !string.IsNullOrWhiteSpace(input) && Uri.TryCreate(input, UriKind.Absolute, out var uri) &&
               (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
    }
}