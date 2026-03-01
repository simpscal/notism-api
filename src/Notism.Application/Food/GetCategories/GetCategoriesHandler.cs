using MediatR;

using Notism.Application.Food.AdminGetCategories;
using Notism.Application.Food.Models;
using Notism.Domain.Food;

namespace Notism.Application.Food.GetCategories;

public class GetCategoriesHandler : IRequestHandler<GetCategoriesRequest, GetCategoriesResponse>
{
    private readonly ICategoryRepository _categoryRepository;

    public GetCategoriesHandler(ICategoryRepository categoryRepository)
    {
        _categoryRepository = categoryRepository;
    }

    public async Task<GetCategoriesResponse> Handle(
        GetCategoriesRequest request,
        CancellationToken cancellationToken)
    {
        var specification = new AdminGetCategoriesSpecification();
        var categories = await _categoryRepository.FilterByExpressionAsync(specification);
        var items = categories
            .Select(c => new CategoryResponse { Id = c.Id, Name = c.Name })
            .ToList();

        return new GetCategoriesResponse
        {
            Items = items,
            TotalCount = items.Count,
        };
    }
}
