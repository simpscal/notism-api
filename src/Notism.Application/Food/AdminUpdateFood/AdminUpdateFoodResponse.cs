using Notism.Application.Common.Constants;
using Notism.Application.Common.Interfaces;
using Notism.Shared.Extensions;

namespace Notism.Application.Food.AdminUpdateFood;

public class AdminUpdateFoodResponse
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public required string Description { get; set; }
    public decimal Price { get; set; }
    public decimal? DiscountPrice { get; set; }
    public required List<string> ImageUrls { get; set; }
    public required string Category { get; set; }
    public bool IsAvailable { get; set; }
    public required string QuantityUnit { get; set; }
    public int StockQuantity { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public static AdminUpdateFoodResponse FromDomain(
        Domain.Food.Food food,
        string categoryName,
        IStorageService storageService)
    {
        return new AdminUpdateFoodResponse
        {
            Id = food.Id,
            Name = food.Name,
            Description = food.Description,
            Price = food.Price,
            DiscountPrice = food.DiscountPrice,
            ImageUrls = food.Images
                .OrderBy(img => img.DisplayOrder)
                .Select(img => storageService.GetPublicUrl(img.FileKey, StorageTypeConstants.Food))
                .ToList(),
            Category = categoryName,
            IsAvailable = food.IsAvailable,
            QuantityUnit = food.QuantityUnit.GetStringValue(),
            StockQuantity = food.StockQuantity,
            CreatedAt = food.CreatedAt,
            UpdatedAt = food.UpdatedAt,
        };
    }
}
