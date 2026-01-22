using MediatR;

using Microsoft.Extensions.Logging;

using Notism.Application.Common.Interfaces;
using Notism.Domain.Food;
using Notism.Domain.Food.Enums;
using Notism.Domain.Food.Specifications;
using Notism.Shared.Extensions;

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
        var category = request.Category?.ToEnum<FoodCategory>();
        var specification = new FoodsFilterSpecification(category, request.Keyword, request.IsAvailable);

        var pagedResult = await _foodRepository.FilterPagedByExpressionAsync(specification, request);

        var items = pagedResult.Items.Select(food => new FoodItemResponse
        {
            Id = food.Id,
            Name = food.Name,
            Description = food.Description,
            Price = food.Price,
            DiscountPrice = food.DiscountPrice,
            ImageUrl = GetImageUrl(food.FileKey),
            Category = food.Category.GetStringValue(),
            IsAvailable = food.IsAvailable,
            QuantityUnit = food.QuantityUnit.GetStringValue(),
            StockQuantity = food.StockQuantity,
        });

        _logger.LogInformation("Retrieved {Count} foods", pagedResult.Items.Count());

        return new GetFoodsResponse
        {
            TotalCount = pagedResult.TotalCount,
            Items = items,
        };
    }

    private string GetImageUrl(string fileKey)
    {
        return string.IsNullOrWhiteSpace(fileKey) ? string.Empty : _storageService.GetPublicUrl(fileKey);
    }
}