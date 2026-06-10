using Notism.Application.Common.Constants;
using Notism.Application.Common.Services;
using Notism.Domain.Order;

namespace Notism.Application.Order.Common;

public sealed record OrderItemResponse
{
    public Guid Id { get; set; }
    public Guid FoodId { get; set; }
    public string FoodName { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
    public decimal? DiscountPrice { get; set; }
    public int Quantity { get; set; }
    public decimal TotalPrice { get; set; }
    public decimal? Surcharge { get; set; }
    public string? CustomisationLabel { get; set; }
    public string? ImageUrl { get; set; }

    public static OrderItemResponse FromDomain(OrderItem item, IStorageService storageService)
    {
        var primaryImage = item.Food.PrimaryImage;
        var imageUrl = primaryImage == null
            ? string.Empty
            : storageService.GetPublicUrl(primaryImage.FileKey, StorageTypeConstants.Food);

        return new OrderItemResponse
        {
            Id = item.Id,
            FoodId = item.FoodId,
            FoodName = item.FoodName,
            UnitPrice = item.UnitPrice,
            DiscountPrice = item.DiscountPrice,
            Quantity = item.Quantity,
            TotalPrice = item.TotalPrice,
            Surcharge = item.Surcharge,
            CustomisationLabel = item.CustomisationLabel,
            ImageUrl = imageUrl,
        };
    }
}
