using Notism.Application.Food.Common;

namespace Notism.Application.Food.GetCategories;

public record GetCategoriesResponse
{
    public required IReadOnlyList<CategoryResponse> Items { get; init; }
    public int TotalCount { get; init; }
}