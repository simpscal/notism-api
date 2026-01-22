using Notism.Shared.Attributes;

namespace Notism.Domain.Food.Enums;

public enum FoodCategory
{
    [StringValue("pizza")]
    Pizza,

    [StringValue("burger")]
    Burger,

    [StringValue("salad")]
    Salad,

    [StringValue("pasta")]
    Pasta,

    [StringValue("dessert")]
    Dessert,

    [StringValue("drink")]
    Drink,

    [StringValue("appetizer")]
    Appetizer,

    [StringValue("soup")]
    Soup,

    [StringValue("sandwich")]
    Sandwich,

    [StringValue("breakfast")]
    Breakfast,

    [StringValue("snack")]
    Snack,
}