using MediatR;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using Notism.Application.Common.Persistence;
using Notism.Application.Common.Services;
using Notism.Domain.Food.Repositories;
using Notism.Shared.Exceptions;

using DomainCategory = Notism.Domain.Food.Category;
using DomainFood = Notism.Domain.Food.Food;

namespace Notism.Application.Food.AdminDeleteCategory;

public class AdminDeleteCategoryHandler : IRequestHandler<AdminDeleteCategoryRequest>
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly IReadDbContext _readDbContext;
    private readonly ILogger<AdminDeleteCategoryHandler> _logger;
    private readonly IMessages _messages;

    public AdminDeleteCategoryHandler(
        ICategoryRepository categoryRepository,
        IReadDbContext readDbContext,
        ILogger<AdminDeleteCategoryHandler> logger,
        IMessages messages)
    {
        _categoryRepository = categoryRepository;
        _readDbContext = readDbContext;
        _logger = logger;
        _messages = messages;
    }

    public async Task Handle(AdminDeleteCategoryRequest request, CancellationToken cancellationToken)
    {
        // Both the category and its foods are loaded TRACKED so the soft-delete mutations
        // persist on SaveChanges via the same context.
        var category = await _readDbContext.Set<DomainCategory>(tracking: true)
                .Where(c => c.Id == request.CategoryId && !c.IsDeleted)
                .FirstOrDefaultAsync(cancellationToken)
            ?? throw new NotFoundException(_messages.CategoryNotFound);

        category.MarkAsDeleted();

        var foodsInCategory = await _readDbContext.Set<DomainFood>(tracking: true)
            .Where(f => f.CategoryId == request.CategoryId && !f.IsDeleted)
            .ToListAsync(cancellationToken);
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