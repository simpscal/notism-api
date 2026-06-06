using Notism.Application.Cart.Common;
using Notism.Domain.Cart;
using Notism.Shared.Extensions;

namespace Notism.Application.Cart.UpdateCartItemCustomisations;

public record UpdateCartItemCustomisationsResponse : CartItemResponse
{
    public static UpdateCartItemCustomisationsResponse FromDomain(CartItem cartItem)
    {
        var customisations = cartItem.Customisations
            .Select(c =>
            {
                var group = cartItem.Food.CustomisationGroups
                    .FirstOrDefault(g => g.Id == c.CustomisationGroupId);
                return CartItemCustomisationResponse.FromDomain(c, group);
            })
            .ToList();

        return new UpdateCartItemCustomisationsResponse
        {
            Id = cartItem.Id,
            FoodId = cartItem.FoodId,
            Name = cartItem.Food.Name,
            Description = cartItem.Food.Description,
            Price = cartItem.Food.Price,
            DiscountPrice = cartItem.Food.DiscountPrice,
            ImageUrl = cartItem.Food.Images.FirstOrDefault()?.FileKey ?? string.Empty,
            Category = cartItem.Food.Category?.Name ?? string.Empty,
            Quantity = cartItem.Quantity,
            StockQuantity = cartItem.Food.StockQuantity,
            QuantityUnit = cartItem.Food.QuantityUnit.GetStringValue(),
            Customisations = customisations,
            TotalSurcharge = cartItem.TotalSurcharge,
        };
    }
}
