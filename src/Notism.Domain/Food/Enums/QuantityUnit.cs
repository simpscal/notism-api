using Notism.Shared.Attributes;

namespace Notism.Domain.Food.Enums;

public enum QuantityUnit
{
    [StringValue("g")]
    Grams,

    [StringValue("ml")]
    Milliliters,
}