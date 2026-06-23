using Notism.Application.Cart.Common;
using Notism.Application.Common.Constants;
using Notism.Application.Common.Services;
using Notism.Domain.Cart;
using Notism.Shared.Extensions;

namespace Notism.Application.Cart.UpdateCartItemCustomisations;

public sealed record UpdateCartItemCustomisationsResponse : CartItemResponse
{
    public static UpdateCartItemCustomisationsResponse FromDomain(
        CartItem cartItem,
        IStorageService storageService)
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
            ImageUrl = GetImageUrl(cartItem.Food.Images, storageService),
            Category = cartItem.Food.Category?.Name ?? string.Empty,
            Quantity = cartItem.Quantity,
            StockQuantity = cartItem.Food.StockQuantity,
            QuantityUnit = cartItem.Food.QuantityUnit.GetStringValue(),
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