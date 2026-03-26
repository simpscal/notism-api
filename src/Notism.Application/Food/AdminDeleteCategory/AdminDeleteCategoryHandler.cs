using MediatR;

using Microsoft.Extensions.Logging;

using Notism.Application.Common.Services;
using Notism.Domain.Common.Specifications;
using Notism.Domain.Food;
using Notism.Shared.Exceptions;

namespace Notism.Application.Food.AdminDeleteCategory;

public class AdminDeleteCategoryHandler : IRequestHandler<AdminDeleteCategoryRequest>
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly IFoodRepository _foodRepository;
    private readonly ILogger<AdminDeleteCategoryHandler> _logger;
    private readonly IMessages _messages;

    public AdminDeleteCategoryHandler(
        ICategoryRepository categoryRepository,
        IFoodRepository foodRepository,
        ILogger<AdminDeleteCategoryHandler> logger,
        IMessages messages)
    {
        _categoryRepository = categoryRepository;
        _foodRepository = foodRepository;
        _logger = logger;
        _messages = messages;
    }

    public async Task Handle(AdminDeleteCategoryRequest request, CancellationToken cancellationToken)
    {
        var specification = new FilterSpecification<Domain.Food.Category>(
            c => c.Id == request.CategoryId && !c.IsDeleted);
        var category = await _categoryRepository.FindByExpressionAsync(specification)
            ?? throw new NotFoundException(_messages.CategoryNotFound);

        category.MarkAsDeleted();

        var foodsInCategorySpec = new FilterSpecification<Domain.Food.Food>(
            f => f.CategoryId == request.CategoryId && !f.IsDeleted);
        var foodsInCategory = (await _foodRepository.FilterByExpressionAsync(foodsInCategorySpec)).ToList();
        foreach (var food in foodsInCategory)
        {
            food.MarkAsDeleted();
        }

        await _categoryRepository.SaveChangesAsync();

        _logger.LogInformation(
            "Soft deleted category {CategoryId} and {FoodCount} associated food(s)",
            request.CategoryId,
            foodsInCategory.Count);
    }
}