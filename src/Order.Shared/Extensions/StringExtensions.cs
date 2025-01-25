namespace Order.Shared.Extensions;

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
}