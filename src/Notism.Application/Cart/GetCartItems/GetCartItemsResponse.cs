using Notism.Application.Cart.Common;
using Notism.Application.Common.Constants;
using Notism.Application.Common.Services;
using Notism.Domain.Cart;

namespace Notism.Application.Cart.GetCartItems;

public sealed record GetCartItemsResponse
{
    public required List<CartItemResponse> Items { get; set; }

    public static GetCartItemsResponse FromDomain(
        IEnumerable<CartItem> cartItems,
        IStorageService storageService)
    {
        return new GetCartItemsResponse
        {
            Items = cartItems
                .Select(cartItem => CartItemResponse.FromDomain(
                    cartItem,
                    cartItem.Food,
                    GetImageUrl(cartItem.Food.Images, storageService)))
                .ToList(),
        };
    }

    private static string GetImageUrl(
        IReadOnlyCollection<Domain.Food.FoodImage> images,
        IStorageService storageService)
    {
        var firstImage = images.FirstOrDefault();
        return firstImage == null
            ? string.Empty
            : storageService.GetPublicUrl(firstImage.FileKey, StorageTypeConstants.Food);
    }
}
