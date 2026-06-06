using Notism.Application.Food.Common;
using Notism.Domain.Food;

namespace Notism.Application.Food.AdminUpdateCategory;

public record AdminUpdateCategoryResponse : CategoryResponse
{
    public static new AdminUpdateCategoryResponse FromDomain(Category category)
    {
        return new AdminUpdateCategoryResponse
        {
            Id = category.Id,
            Name = category.Name,
        };
    }
}
