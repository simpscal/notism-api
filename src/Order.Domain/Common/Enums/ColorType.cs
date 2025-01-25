using Order.Shared.Attributes;

namespace Order.Domain.Common.Enums;

public enum ColorType
{
    [StringValue("black")]
    Black,
    [StringValue("white")]
    White,
}