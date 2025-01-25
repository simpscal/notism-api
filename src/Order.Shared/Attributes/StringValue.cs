namespace Order.Shared.Attributes;

[AttributeUsage(AttributeTargets.Field)]
public sealed class StringValue(string value) : Attribute
{
    public string Value { get; } = value;
}