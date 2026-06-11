using Notism.Application.Common.Persistence;
using Notism.Shared.Enums;
using Notism.Shared.Extensions;

using DomainFood = Notism.Domain.Food.Food;

namespace Notism.Application.Food.GetFoods;

/// <summary>
/// Self-contained read for the public food list: category/keyword/availability filter,
/// sort, page window and the projection to <see cref="FoodListProjection"/> all execute
/// server-side over the no-tracking food set. Owned by <see cref="GetFoodsHandler"/>;
/// every predicate, ordering and projection is duplicated inline here rather than shared
/// with any other handler.
/// </summary>
public sealed class GetFoodsQuery
{
    private readonly IReadDbContext _readDbContext;

    public GetFoodsQuery(IReadDbContext readDbContext)
    {
        _readDbContext = readDbContext;
    }

    public async Task<(int TotalCount, List<FoodListProjection> Items)> ExecuteAsync(
        string? category,
        string? keyword,
        bool? isAvailable,
        string? sortBy,
        string? sortOrder,
        int skip,
        int take,
        CancellationToken cancellationToken = default)
    {
        var isDescending = (sortOrder?.FromCamelCase<SortOrder>() ?? SortOrder.Asc) == SortOrder.Desc;

        var filtered = _readDbContext.Set<DomainFood>()
            .Where(food =>
                !food.IsDeleted &&
                (string.IsNullOrWhiteSpace(category) || (food.Category != null && food.Category.Name == category)) &&
                (!isAvailable.HasValue || food.IsAvailable == isAvailable.Value) &&
                (string.IsNullOrWhiteSpace(keyword) ||
                    food.Name.ToLower().Contains(keyword) ||
                    food.Description.ToLower().Contains(keyword)));

        filtered = sortBy switch
        {
            "name" => isDescending
                ? filtered.OrderByDescending(f => f.Name)
                : filtered.OrderBy(f => f.Name),
            "price" => isDescending
                ? filtered.OrderByDescending(f => f.Price)
                : filtered.OrderBy(f => f.Price),
            "discountPrice" => isDescending
                ? filtered.OrderByDescending(f => f.DiscountPrice)
                : filtered.OrderBy(f => f.DiscountPrice),
            _ => filtered.OrderByDescending(f => f.CreatedAt),
        };

        var totalCount = await _readDbContext.CountAsync(filtered, cancellationToken);

        var projected = filtered
            .Skip(skip)
            .Take(take)
            .Select(f => new FoodListProjection
            {
                Id = f.Id,
                Name = f.Name,
                Description = f.Description,
                Price = f.Price,
                DiscountPrice = f.DiscountPrice,
                CategoryName = f.Category != null ? f.Category.Name : string.Empty,
                IsAvailable = f.IsAvailable,
                QuantityUnit = f.QuantityUnit,
                StockQuantity = f.StockQuantity,
                ImageKeys = f.Images
                    .OrderBy(i => i.DisplayOrder)
                    .Select(i => new ImageKeyOrder(i.FileKey, i.DisplayOrder))
                    .ToList(),
            });

        var items = await _readDbContext.ToListAsync(projected, cancellationToken);

        return (totalCount, items);
    }
}
