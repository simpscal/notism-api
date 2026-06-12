using MediatR;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using Notism.Application.Common.Persistence;
using Notism.Application.Common.Services;
using Notism.Domain.Food.Enums;
using Notism.Domain.Food.Repositories;
using Notism.Shared.Exceptions;
using Notism.Shared.Extensions;

using DomainCategory = Notism.Domain.Food.Category;

namespace Notism.Application.Food.AdminAddFood;

public class AdminAddFoodHandler : IRequestHandler<AdminAddFoodRequest, AdminAddFoodResponse>
{
    private readonly IFoodRepository _foodRepository;
    private readonly IReadDbContext _readDbContext;
    private readonly IStorageService _storageService;
    private readonly ILogger<AdminAddFoodHandler> _logger;
    private readonly IMessages _messages;

    public AdminAddFoodHandler(
        IFoodRepository foodRepository,
        IReadDbContext readDbContext,
        IStorageService storageService,
        ILogger<AdminAddFoodHandler> logger,
        IMessages messages)
    {
        _foodRepository = foodRepository;
        _readDbContext = readDbContext;
        _storageService = storageService;
        _logger = logger;
        _messages = messages;
    }

    public async Task<AdminAddFoodResponse> Handle(
        AdminAddFoodRequest request,
        CancellationToken cancellationToken)
    {
        var categoryName = request.Category.Trim();
        var category = await _readDbContext.Set<DomainCategory>()
                .Where(c => c.Name == categoryName && !c.IsDeleted)
                .FirstOrDefaultAsync(cancellationToken)
            ?? throw new ResultFailureException(_messages.CategoryNotFound);

        var quantityUnit = request.QuantityUnit.ToEnum<QuantityUnit>();

        if (request.DiscountPrice.HasValue && request.DiscountPrice.Value >= request.Price)
        {
            throw new ResultFailureException(_messages.DiscountPriceMustBeLess);
        }

        var food = Domain.Food.Food.Create(
            request.Name,
            request.Description,
            request.Price,
            category.Id,
            quantityUnit,
            request.StockQuantity,
            request.DiscountPrice);

        if (!request.IsAvailable)
        {
            food.SetAvailability(false);
        }

        if (request.Images != null && request.Images.Count > 0)
        {
            foreach (var img in request.Images)
            {
                food.AddImage(img.FileKey, img.DisplayOrder, img.AltText);
            }
        }

        _foodRepository.Add(food);
        await _foodRepository.SaveChangesAsync();

        _logger.LogInformation("Added food {FoodId} with name {Name}", food.Id, food.Name);

        return AdminAddFoodResponse.FromDomain(food, category.Name, _storageService);
    }
}