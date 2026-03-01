using Notism.Application.Food.Models;

namespace Notism.Application.Food.GetCategories;

public record GetCategoriesResponse
{
    public required IReadOnlyList<CategoryResponse> Items { get; init; }
    public int TotalCount { get; init; }
}
