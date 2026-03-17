using MediatR;

using Microsoft.Extensions.Logging;

using Notism.Domain.Common.Specifications;
using Notism.Domain.Food;
using Notism.Shared.Exceptions;

namespace Notism.Application.Food.AdminAddCategory;

public class AdminAddCategoryHandler : IRequestHandler<AdminAddCategoryRequest, AdminAddCategoryResponse>
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly ILogger<AdminAddCategoryHandler> _logger;

    public AdminAddCategoryHandler(
        ICategoryRepository categoryRepository,
        ILogger<AdminAddCategoryHandler> logger)
    {
        _categoryRepository = categoryRepository;
        _logger = logger;
    }

    public async Task<AdminAddCategoryResponse> Handle(
        AdminAddCategoryRequest request,
        CancellationToken cancellationToken)
    {
        var nameTrimmed = request.Name.Trim();
        var existingSpec = new FilterSpecification<Notism.Domain.Food.Category>(
            c => !c.IsDeleted && c.Name.ToLower() == nameTrimmed.ToLower());
        var existing = await _categoryRepository.FindByExpressionAsync(existingSpec);
        if (existing != null)
        {
            throw new ResultFailureException("A category with this name already exists.");
        }

        var category = Notism.Domain.Food.Category.Create(request.Name);

        _categoryRepository.Add(category);
        await _categoryRepository.SaveChangesAsync();

        _logger.LogInformation("Added category {CategoryId} with name {Name}", category.Id, category.Name);

        return new AdminAddCategoryResponse
        {
            Id = category.Id,
            Name = category.Name,
        };
    }
}