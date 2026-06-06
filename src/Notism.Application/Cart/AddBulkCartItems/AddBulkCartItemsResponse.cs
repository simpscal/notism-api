using Notism.Application.Cart.Common;
using Notism.Application.Common.Constants;
using Notism.Application.Common.Interfaces;
using Notism.Domain.Cart;

namespace Notism.Application.Cart.AddBulkCartItems;

public sealed record AddBulkCartItemsResponse
{
    public required List<CartItemResponse> Items { get; set; }

    public static AddBulkCartItemsResponse FromDomain(
        IEnumerable<(CartItem CartItem, Domain.Food.Food Food)> addedCartItems,
        IStorageService storageService)
    {
        return new AddBulkCartItemsResponse
        {
            Items = addedCartItems
                .Select(aci => CartItemResponse.FromDomain(
                    aci.CartItem,
                    aci.Food,
                    GetImageUrl(aci.Food.Images, storageService),
                    new List<CartItemCustomisationResponse>()))
                .ToList(),
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