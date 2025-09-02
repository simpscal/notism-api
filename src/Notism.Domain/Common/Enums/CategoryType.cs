using Notism.Shared.Attributes;

namespace Notism.Domain.Common.Enums;

public enum CategoryType
{
    [StringValue("top")]
    Top,
    [StringValue("bottom")]
    Bottom,
    [StringValue("accessory")]
    Accessory,
}