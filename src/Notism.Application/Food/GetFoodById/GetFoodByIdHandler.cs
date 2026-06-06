using MediatR;

using Microsoft.Extensions.Logging;

using Notism.Application.Common.Constants;
using Notism.Application.Common.Interfaces;
using Notism.Application.Common.Services;
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
    private readonly IMessages _messages;

    public GetFoodByIdHandler(
        IRepository<Domain.Food.Food> foodRepository,
        IStorageService storageService,
        ILogger<GetFoodByIdHandler> logger,
        IMessages messages)
    {
        _foodRepository = foodRepository;
        _storageService = storageService;
        _logger = logger;
        _messages = messages;
    }

    public async Task<GetFoodByIdResponse> Handle(
        GetFoodByIdRequest request,
        CancellationToken cancellationToken)
    {
        var specification = new FilterSpecification<Domain.Food.Food>(f => f.Id == request.FoodId && !f.IsDeleted)
            .Include(f => f.Images)
            .Include("Category")
            .Include("CustomisationGroups.Options");
        var food = await _foodRepository.FindByExpressionAsync(specification);
        if (food == null)
        {
            throw new ResultFailureException(_messages.FoodNotFound);
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
            Customisations = GetCustomisations(food.CustomisationGroups, request.IncludeEmptyGroups),
        };
    }

    private List<string> GetImageUrls(IReadOnlyCollection<Domain.Food.FoodImage> images)
    {
        return images
            .OrderBy(img => img.DisplayOrder)
            .Select(img => GetDetailImageUrl(img.FileKey))
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
                ImageUrl = GetDetailImageUrl(img.FileKey),
            })
            .ToList();
    }

    private static List<FoodCustomisationGroupResponse> GetCustomisations(
        IReadOnlyCollection<Domain.Food.FoodCustomisationGroup> groups,
        bool includeEmptyGroups = false)
    {
        return groups
            .Where(g => includeEmptyGroups || g.Options.Count > 0)
            .OrderBy(g => g.DisplayOrder)
            .Select(g => new FoodCustomisationGroupResponse
            {
                Id = g.Id,
                Label = g.Label,
                Required = g.IsRequired,
                Options = g.Options
                    .OrderBy(o => o.DisplayOrder)
                    .Select(o => new FoodCustomisationOptionResponse
                    {
                        Value = o.Id,
                        Label = o.Label,
                        Surcharge = o.Surcharge.HasValue && o.Surcharge.Value != 0m ? o.Surcharge : null,
                    })
                    .ToList(),
            })
            .ToList();
    }

    private string GetDetailImageUrl(string fileKey)
    {
        return _storageService.GetPublicUrl(fileKey, StorageTypeConstants.FoodDetail);
    }
}