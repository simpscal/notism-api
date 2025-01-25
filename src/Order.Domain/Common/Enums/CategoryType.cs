using Order.Shared.Attributes;

namespace Order.Domain.Common.Enums;

public enum CategoryType
{
    [StringValue("top")]
    Top,
    [StringValue("bottom")]
    Bottom,
    [StringValue("accessory")]
    Accessory,
}