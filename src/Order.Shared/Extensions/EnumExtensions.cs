using System.Reflection;

using Order.Shared.Attributes;

namespace Order.Shared.Extensions;

public static class EnumExtensions
{
    public static T ToEnum<T>(this string value)
        where T : Enum
    {
        foreach (var field in typeof(T).GetFields())
        {
            var attribute = field.GetCustomAttribute<StringValue>();
            if (attribute?.Value == value)
            {
                return ((T)field.GetValue(null)!)!;
            }
        }

        throw new ArgumentException($"No enum with string value '{value}' found in {typeof(T)}");
    }

    public static bool ExistInEnum<T>(this string value)
        where T : Enum
    {
        foreach (var field in typeof(T).GetFields())
        {
            var attribute = field.GetCustomAttribute<StringValue>();
            if (attribute?.Value == value)
            {
                return true;
            }
        }

        return false;
    }

    public static string GetStringValue(this Enum value)
    {
        var field = value.GetType().GetField(value.ToString());
        var attribute = field?.GetCustomAttribute<StringValue>();
        return attribute?.Value ?? value.ToString();
    }
}