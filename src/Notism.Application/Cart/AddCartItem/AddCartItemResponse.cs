using Notism.Application.Cart.Common;
using Notism.Application.Common.Constants;
using Notism.Application.Common.Services;
using Notism.Domain.Cart;
using Notism.Shared.Extensions;

namespace Notism.Application.Cart.AddCartItem;

public sealed record AddCartItemResponse : CartItemResponse
{
    public static AddCartItemResponse FromDomain(
        CartItem cartItem,
        Domain.Food.Food cartFood,
        Domain.Food.Food fullFood,
        IStorageService storageService)
    {
        var customisations = cartItem.Customisations
            .Select(c =>
            {
                var group = fullFood.CustomisationGroups.FirstOrDefault(g => g.Id == c.CustomisationGroupId);
                var availableOptions = group?.Options
                    .OrderBy(o => o.DisplayOrder)
                    .Select(CartItemAvailableOptionResponse.FromDomain)
                    .ToList() ?? new List<CartItemAvailableOptionResponse>();
                return CartItemCustomisationResponse.FromDomain(c, availableOptions);
            })
            .ToList();

        return new AddCartItemResponse
        {
            Id = cartItem.Id,
            FoodId = cartItem.FoodId,
            Name = cartFood.Name,
            Description = cartFood.Description,
            Price = cartFood.Price,
            DiscountPrice = cartFood.DiscountPrice,
            ImageUrl = GetImageUrl(cartFood.Images, storageService),
            Category = cartFood.Category?.Name ?? string.Empty,
            Quantity = cartItem.Quantity,
            StockQuantity = cartFood.StockQuantity,
            QuantityUnit = cartFood.QuantityUnit.GetStringValue(),
            Customisations = customisations,
            TotalSurcharge = cartItem.TotalSurcharge,
        };
    }

    private static string GetImageUrl(
        IReadOnlyCollection<Domain.Food.FoodImage> images,
        IStorageService storageService)
    {
        var firstImage = images.OrderBy(img => img.DisplayOrder).FirstOrDefault();
        return firstImage == null
            ? string.Empty
            : storageService.GetPublicUrl(firstImage.FileKey, StorageTypeConstants.Food);
    }
}