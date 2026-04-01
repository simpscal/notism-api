using MediatR;

using Microsoft.Extensions.Logging;

using Notism.Application.Common.Constants;
using Notism.Application.Common.Interfaces;
using Notism.Domain.Food;
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
            f => new
            {
                f.Id,
                f.Name,
                f.Description,
                f.Price,
                f.DiscountPrice,
                CategoryName = f.Category != null ? f.Category.Name : string.Empty,
                f.IsAvailable,
                f.QuantityUnit,
                f.StockQuantity,
                ImageKeys = f.Images
                    .OrderBy(i => i.DisplayOrder)
                    .Select(i => new ImageKeyOrder(i.FileKey, i.DisplayOrder))
                    .ToList(),
            });

        var items = pagedResult.Items.Select(proj => new FoodItemResponse
        {
            Id = proj.Id,
            Name = proj.Name,
            Description = proj.Description,
            Price = proj.Price,
            DiscountPrice = proj.DiscountPrice,
            ImageUrl = GetImageUrlFromKeys(proj.ImageKeys, _storageService),
            Category = proj.CategoryName,
            IsAvailable = proj.IsAvailable,
            QuantityUnit = proj.QuantityUnit.GetStringValue(),
            StockQuantity = proj.StockQuantity,
        });

        _logger.LogInformation("Retrieved {Count} foods", pagedResult.Items.Count());

        return new GetFoodsResponse
        {
            TotalCount = pagedResult.TotalCount,
            Items = items,
        };
    }

    private static string GetImageUrlFromKeys(IEnumerable<ImageKeyOrder> imageKeys, IStorageService storageService)
    {
        var first = imageKeys.OrderBy(k => k.DisplayOrder).FirstOrDefault();
        return first == null ? string.Empty : storageService.GetPublicUrl(first.FileKey, StorageTypeConstants.Food);
    }
}

internal record ImageKeyOrder(string FileKey, int DisplayOrder);