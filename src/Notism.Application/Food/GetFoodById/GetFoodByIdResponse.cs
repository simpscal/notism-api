using Notism.Application.Common.Constants;
using Notism.Application.Common.Interfaces;
using Notism.Domain.Food;
using Notism.Shared.Extensions;

namespace Notism.Application.Food.GetFoodById;

public sealed record GetFoodByIdResponse
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public required string Description { get; set; }
    public decimal Price { get; set; }
    public decimal? DiscountPrice { get; set; }
    public required List<string> ImageUrls { get; set; }
    public required List<FoodImageResponse> Images { get; set; }
    public required string Category { get; set; }
    public bool IsAvailable { get; set; }
    public required string QuantityUnit { get; set; }
    public int StockQuantity { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public required List<FoodCustomisationGroupResponse> Customisations { get; set; }

    public static GetFoodByIdResponse FromDomain(
        Domain.Food.Food food,
        IStorageService storageService,
        bool includeEmptyGroups)
    {
        var orderedImages = food.Images
            .OrderBy(img => img.DisplayOrder)
            .ToList();

        return new GetFoodByIdResponse
        {
            Id = food.Id,
            Name = food.Name,
            Description = food.Description,
            Price = food.Price,
            DiscountPrice = food.DiscountPrice,
            ImageUrls = orderedImages
                .Select(img => storageService.GetPublicUrl(img.FileKey, StorageTypeConstants.FoodDetail))
                .ToList(),
            Images = orderedImages
                .Select(img => FoodImageResponse.FromDomain(img, storageService))
                .ToList(),
            Category = food.Category?.Name ?? string.Empty,
            IsAvailable = food.IsAvailable,
            QuantityUnit = food.QuantityUnit.GetStringValue(),
            StockQuantity = food.StockQuantity,
            CreatedAt = food.CreatedAt,
            UpdatedAt = food.UpdatedAt,
            Customisations = food.CustomisationGroups
                .Where(g => includeEmptyGroups || g.Options.Count > 0)
                .OrderBy(g => g.DisplayOrder)
                .Select(FoodCustomisationGroupResponse.FromDomain)
                .ToList(),
        };
    }
}

public sealed record FoodImageResponse
{
    public required string FileKey { get; set; }
    public int DisplayOrder { get; set; }
    public string? AltText { get; set; }
    public required string ImageUrl { get; set; }

    public static FoodImageResponse FromDomain(FoodImage image, IStorageService storageService)
    {
        return new FoodImageResponse
        {
            FileKey = image.FileKey,
            DisplayOrder = image.DisplayOrder,
            AltText = image.AltText,
            ImageUrl = storageService.GetPublicUrl(image.FileKey, StorageTypeConstants.FoodDetail),
        };
    }
}

public sealed record FoodCustomisationGroupResponse
{
    public Guid Id { get; set; }
    public required string Label { get; set; }
    public bool Required { get; set; }
    public required List<FoodCustomisationOptionResponse> Options { get; set; }

    public static FoodCustomisationGroupResponse FromDomain(FoodCustomisationGroup group)
    {
        return new FoodCustomisationGroupResponse
        {
            Id = group.Id,
            Label = group.Label,
            Required = group.IsRequired,
            Options = group.Options
                .OrderBy(o => o.DisplayOrder)
                .Select(FoodCustomisationOptionResponse.FromDomain)
                .ToList(),
        };
    }
}

public sealed record FoodCustomisationOptionResponse
{
    public Guid Value { get; set; }
    public required string Label { get; set; }
    public decimal? Surcharge { get; set; }

    public static FoodCustomisationOptionResponse FromDomain(FoodCustomisationOption option)
    {
        return new FoodCustomisationOptionResponse
        {
            Value = option.Id,
            Label = option.Label,
            Surcharge = option.Surcharge.HasValue && option.Surcharge.Value != 0m ? option.Surcharge : null,
        };
    }
}