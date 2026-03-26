using MediatR;

using Microsoft.Extensions.Logging;

using Notism.Application.Common.Services;
using Notism.Domain.Common.Specifications;
using Notism.Domain.Food;
using Notism.Shared.Exceptions;

namespace Notism.Application.Food.AdminUpdateCategory;

public class AdminUpdateCategoryHandler : IRequestHandler<AdminUpdateCategoryRequest, AdminUpdateCategoryResponse>
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly ILogger<AdminUpdateCategoryHandler> _logger;
    private readonly IMessages _messages;

    public AdminUpdateCategoryHandler(
        ICategoryRepository categoryRepository,
        ILogger<AdminUpdateCategoryHandler> logger,
        IMessages messages)
    {
        _categoryRepository = categoryRepository;
        _logger = logger;
        _messages = messages;
    }

    public async Task<AdminUpdateCategoryResponse> Handle(
        AdminUpdateCategoryRequest request,
        CancellationToken cancellationToken)
    {
        var categorySpec = new FilterSpecification<Notism.Domain.Food.Category>(
            c => c.Id == request.CategoryId && !c.IsDeleted);
        var category = await _categoryRepository.FindByExpressionAsync(categorySpec)
            ?? throw new NotFoundException(_messages.CategoryNotFound);

        var nameTrimmed = request.Name.Trim();
        var duplicateSpec = new FilterSpecification<Notism.Domain.Food.Category>(
            c => !c.IsDeleted && c.Id != request.CategoryId && c.Name.ToLower() == nameTrimmed.ToLower());
        var duplicate = await _categoryRepository.FindByExpressionAsync(duplicateSpec);
        if (duplicate != null)
        {
            throw new ResultFailureException(_messages.CategoryAlreadyExists);
        }

        category.UpdateName(request.Name);
        await _categoryRepository.SaveChangesAsync();

        _logger.LogInformation("Updated category {CategoryId} to name {Name}", category.Id, category.Name);

        return new AdminUpdateCategoryResponse
        {
            Id = category.Id,
            Name = category.Name,
        };
    }
}