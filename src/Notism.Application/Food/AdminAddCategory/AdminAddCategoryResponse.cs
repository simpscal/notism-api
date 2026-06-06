using Notism.Application.Food.Common;
using Notism.Domain.Food;

namespace Notism.Application.Food.AdminAddCategory;

public record AdminAddCategoryResponse : CategoryResponse
{
    public static new AdminAddCategoryResponse FromDomain(Category category)
    {
        return new AdminAddCategoryResponse
        {
            Id = category.Id,
            Name = category.Name,
        };
    }
}
