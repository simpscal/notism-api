using MediatR;

using Notism.Application.Common.Behaviors;
using Notism.Application.Common.Constants;
using Notism.Application.Food.AdminUpdateFood;

namespace Notism.Application.Food.AdminAddFood;

public record AdminAddFoodRequest : IRequest<AdminAddFoodResponse>, ICacheInvalidatingRequest
{
    public IEnumerable<string> CacheTagsToEvict => [CacheTagConstants.Foods];

    public required string Name { get; set; }
    public required string Description { get; set; }
    public required decimal Price { get; set; }
    public decimal? DiscountPrice { get; set; }
    public required string Category { get; set; }
    public bool IsAvailable { get; set; } = true;
    public required int StockQuantity { get; set; }
    public required string QuantityUnit { get; set; }
    public List<FoodImageRequest>? Images { get; set; }
}