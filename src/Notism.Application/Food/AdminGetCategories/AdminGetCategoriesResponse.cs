using Notism.Application.Food.Models;

namespace Notism.Application.Food.AdminGetCategories;

public record AdminGetCategoriesResponse
{
    public required IReadOnlyList<CategoryResponse> Items { get; init; }
    public int TotalCount { get; init; }
}
