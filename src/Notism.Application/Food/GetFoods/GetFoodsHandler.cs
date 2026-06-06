using MediatR;

using Microsoft.Extensions.Logging;

using Notism.Application.Common.Interfaces;
using Notism.Domain.Food;

namespace Notism.Application.Food.GetFoods;

public class GetFoodsHandler : IRequestHandler<GetFoodsRequest, GetFoodsResponse>
{
    private readonly IFoodRepository _foodRepository;
    private readonly IStorageService _storageService;
    private readonly ILogger<GetFoodsHandler> _logger;

    public GetFoodsHandler(
        IFoodRepository foodRepository,
        IStorageService storageService,
        ILogger<GetFoodsHandler> logger)
    {
        _foodRepository = foodRepository;
        _storageService = storageService;
        _logger = logger;
    }

    public async Task<GetFoodsResponse> Handle(
        GetFoodsRequest request,
        CancellationToken cancellationToken)
    {
        var keywordLower = request.Keyword?.ToLower();
        var categoryFilter = request.Category?.Trim();
        var specification = new GetFoodsSpecification(
            categoryFilter,
            keywordLower,
            request.IsAvailable,
            request.SortBy,
            request.SortOrder);

        var pagedResult = await _foodRepository.FilterPagedByExpressionAsync(
            specification,
            request,
            f => new FoodListProjection
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

        var items = pagedResult.Items
            .Select(proj => FoodItemResponse.FromProjection(proj, _storageService));

        _logger.LogInformation("Retrieved {Count} foods", pagedResult.Items.Count());

        return new GetFoodsResponse
        {
            TotalCount = pagedResult.TotalCount,
            Items = items,
        };
    }
}