using Notism.Application.Food.Common;
using Notism.Domain.Food;

namespace Notism.Application.Food.AdminGetCategoryDetail;

public sealed record AdminGetCategoryDetailResponse : CategoryResponse
{
    public static new AdminGetCategoryDetailResponse FromDomain(Category category)
    {
        return new AdminGetCategoryDetailResponse
        {
            Id = category.Id,
            Name = category.Name,
        };
    }
}