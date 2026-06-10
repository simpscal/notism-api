using Notism.Application.Common.Constants;
using Notism.Application.Common.Services;
using Notism.Shared.Extensions;
using Notism.Shared.Models;

namespace Notism.Application.Food.GetFoods;

public sealed record GetFoodsResponse : PagedResult<FoodItemResponse>;

public sealed record FoodItemResponse
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public required string Description { get; set; }
    public decimal Price { get; set; }
    public decimal? DiscountPrice { get; set; }
    public required string ImageUrl { get; set; }
    public required string Category { get; set; }
    public bool IsAvailable { get; set; }
    public required string QuantityUnit { get; set; }
    public int StockQuantity { get; set; }

    public static FoodItemResponse FromProjection(FoodListProjection projection, IStorageService storageService)
    {
        var firstImage = projection.ImageKeys.OrderBy(k => k.DisplayOrder).FirstOrDefault();
        var imageUrl = firstImage == null
            ? string.Empty
            : storageService.GetPublicUrl(firstImage.FileKey, StorageTypeConstants.Food);

        return new FoodItemResponse
        {
            Id = projection.Id,
            Name = projection.Name,
            Description = projection.Description,
            Price = projection.Price,
            DiscountPrice = projection.DiscountPrice,
            ImageUrl = imageUrl,
            Category = projection.CategoryName,
            IsAvailable = projection.IsAvailable,
            QuantityUnit = projection.QuantityUnit.GetStringValue(),
            StockQuantity = projection.StockQuantity,
        };
    }
}
