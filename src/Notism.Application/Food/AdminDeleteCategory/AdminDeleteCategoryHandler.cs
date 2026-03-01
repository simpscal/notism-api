using MediatR;

using Microsoft.Extensions.Logging;

using Notism.Domain.Common.Specifications;
using Notism.Domain.Food;
using Notism.Shared.Exceptions;

namespace Notism.Application.Food.AdminDeleteCategory;

public class AdminDeleteCategoryHandler : IRequestHandler<AdminDeleteCategoryRequest>
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly ILogger<AdminDeleteCategoryHandler> _logger;

    public AdminDeleteCategoryHandler(
        ICategoryRepository categoryRepository,
        ILogger<AdminDeleteCategoryHandler> logger)
    {
        _categoryRepository = categoryRepository;
        _logger = logger;
    }

    public async Task Handle(AdminDeleteCategoryRequest request, CancellationToken cancellationToken)
    {
        var specification = new FilterSpecification<Notism.Domain.Food.Category>(
            c => c.Id == request.CategoryId && !c.IsDeleted);
        var category = await _categoryRepository.FindByExpressionAsync(specification)
            ?? throw new NotFoundException("Category not found.");

        category.MarkAsDeleted();
        await _categoryRepository.SaveChangesAsync();

        _logger.LogInformation("Soft deleted category {CategoryId}", request.CategoryId);
    }
}
