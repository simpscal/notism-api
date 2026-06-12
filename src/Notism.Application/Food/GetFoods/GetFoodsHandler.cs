using MediatR;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using Notism.Application.Common.Persistence;
using Notism.Application.Common.Services;
using Notism.Shared.Enums;
using Notism.Shared.Extensions;

using DomainFood = Notism.Domain.Food.Food;

namespace Notism.Application.Food.GetFoods;

public class GetFoodsHandler : IRequestHandler<GetFoodsRequest, GetFoodsResponse>
{
    private readonly IReadDbContext _readDbContext;
    private readonly IStorageService _storageService;
    private readonly ILogger<GetFoodsHandler> _logger;

    public GetFoodsHandler(
        IReadDbContext readDbContext,
        IStorageService storageService,
        ILogger<GetFoodsHandler> logger)
    {
        _readDbContext = readDbContext;
        _storageService = storageService;
        _logger = logger;
    }

    public async Task<GetFoodsResponse> Handle(
        GetFoodsRequest request,
        CancellationToken cancellationToken)
    {
        var keyword = request.Keyword?.ToLower();
        var category = request.Category?.Trim();
        var isAvailable = request.IsAvailable;

        var isDescending = (request.SortOrder?.FromCamelCase<SortOrder>() ?? SortOrder.Asc) == SortOrder.Desc;

        var filtered = _readDbContext.Set<DomainFood>()
            .Where(food =>
                !food.IsDeleted &&
                (string.IsNullOrWhiteSpace(category) || (food.Category != null && food.Category.Name == category)) &&
                (!isAvailable.HasValue || food.IsAvailable == isAvailable.Value) &&
                (string.IsNullOrWhiteSpace(keyword) ||
                    food.Name.ToLower().Contains(keyword) ||
                    food.Description.ToLower().Contains(keyword)));

        filtered = request.SortBy switch
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

        var totalCount = await filtered.CountAsync(cancellationToken);

        var projections = await filtered
            .Skip(request.Skip)
            .Take(request.Take)
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
            })
            .ToListAsync(cancellationToken);

        var items = projections
            .Select(proj => FoodItemResponse.FromProjection(proj, _storageService));

        _logger.LogInformation("Retrieved {Count} foods", projections.Count);

        return new GetFoodsResponse
        {
            TotalCount = totalCount,
            Items = items,
        };
    }
}