using Order.Shared.Attributes;

namespace Order.Domain.Common.Enums;

public enum SubCategoryType
{
    [StringValue("teeShirt")]
    TeeShirt,
    [StringValue("shirt")]
    Shirt,
    [StringValue("sweatShirt")]
    SweatShirt,
    [StringValue("longSleeve")]
    LongSleeve,
    [StringValue("polo")]
    Polo,
    [StringValue("jacket")]
    Jacket,

    [StringValue("pant")]
    Pant,
    [StringValue("jean")]
    Jean,
}