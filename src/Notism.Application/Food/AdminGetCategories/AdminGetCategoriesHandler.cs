using MediatR;

using Microsoft.Extensions.Logging;

using Notism.Application.Food.Common;
using Notism.Domain.Food;
using Notism.Domain.Food.Repositories;

namespace Notism.Application.Food.AdminGetCategories;

public class AdminGetCategoriesHandler : IRequestHandler<AdminGetCategoriesRequest, AdminGetCategoriesResponse>
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly ILogger<AdminGetCategoriesHandler> _logger;

    public AdminGetCategoriesHandler(
        ICategoryRepository categoryRepository,
        ILogger<AdminGetCategoriesHandler> logger)
    {
        _categoryRepository = categoryRepository;
        _logger = logger;
    }

    public async Task<AdminGetCategoriesResponse> Handle(
        AdminGetCategoriesRequest request,
        CancellationToken cancellationToken)
    {
        var specification = new AdminGetCategoriesSpecification();
        var categories = await _categoryRepository.FilterByExpressionAsync(specification);
        var items = categories
            .Select(CategoryResponse.FromDomain)
            .ToList();

        _logger.LogInformation("Retrieved {Count} categories for admin", items.Count);

        return new AdminGetCategoriesResponse
        {
            Items = items,
            TotalCount = items.Count,
        };
    }
}