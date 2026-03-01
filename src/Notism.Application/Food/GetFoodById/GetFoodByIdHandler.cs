using MediatR;

using Microsoft.Extensions.Logging;

using Notism.Application.Common.Interfaces;
using Notism.Domain.Common.Interfaces;
using Notism.Domain.Common.Specifications;
using Notism.Shared.Exceptions;
using Notism.Shared.Extensions;

namespace Notism.Application.Food.GetFoodById;

public class GetFoodByIdHandler : IRequestHandler<GetFoodByIdRequest, GetFoodByIdResponse>
{
    private readonly IRepository<Domain.Food.Food> _foodRepository;
    private readonly IStorageService _storageService;
    private readonly ILogger<GetFoodByIdHandler> _logger;

    public GetFoodByIdHandler(
        IRepository<Domain.Food.Food> foodRepository,
        IStorageService storageService,
        ILogger<GetFoodByIdHandler> logger)
    {
        _foodRepository = foodRepository;
        _storageService = storageService;
        _logger = logger;
    }

    public async Task<GetFoodByIdResponse> Handle(
        GetFoodByIdRequest request,
        CancellationToken cancellationToken)
    {
        var specification = new FilterSpecification<Domain.Food.Food>(f => f.Id == request.FoodId && !f.IsDeleted)
            .Include(f => f.Images)
            .Include("Category");
        var food = await _foodRepository.FindByExpressionAsync(specification);
        if (food == null)
        {
            throw new ResultFailureException("Food not found");
        }

        _logger.LogInformation("Retrieved food {FoodId}", request.FoodId);

        return new GetFoodByIdResponse
        {
            Id = food.Id,
            Name = food.Name,
            Description = food.Description,
            Price = food.Price,
            DiscountPrice = food.DiscountPrice,
            ImageUrls = GetImageUrls(food.Images),
            Images = GetImages(food.Images),
            Category = food.Category?.Name ?? string.Empty,
            IsAvailable = food.IsAvailable,
            QuantityUnit = food.QuantityUnit.GetStringValue(),
            StockQuantity = food.StockQuantity,
            CreatedAt = food.CreatedAt,
            UpdatedAt = food.UpdatedAt,
        };
    }

    private List<string> GetImageUrls(IReadOnlyCollection<Domain.Food.FoodImage> images)
    {
        return images
            .OrderBy(img => img.DisplayOrder)
            .Select(img => _storageService.GetPublicUrl(img.FileKey))
            .ToList();
    }

    private List<FoodImageResponse> GetImages(IReadOnlyCollection<Domain.Food.FoodImage> images)
    {
        return images
            .OrderBy(img => img.DisplayOrder)
            .Select(img => new FoodImageResponse
            {
                FileKey = img.FileKey,
                DisplayOrder = img.DisplayOrder,
                AltText = img.AltText,
                ImageUrl = _storageService.GetPublicUrl(img.FileKey),
            })
            .ToList();
    }
}