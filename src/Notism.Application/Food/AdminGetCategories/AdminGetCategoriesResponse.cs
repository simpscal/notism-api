using Notism.Application.Food.Common;

namespace Notism.Application.Food.AdminGetCategories;

public sealed record AdminGetCategoriesResponse
{
    public required IReadOnlyList<CategoryResponse> Items { get; init; }
    public int TotalCount { get; init; }
}