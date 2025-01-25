using Microsoft.EntityFrameworkCore;

using Order.Domain.Common.Enums;
using Order.Domain.SubCategory;
using Order.Infrastructure.Common;
using Order.Shared.Extensions;

namespace Order.Infrastructure.SubCategories;

public class SubCategoryRepository(AppDbContext appDbContext) :
    Repository<SubCategory>(appDbContext),
    ISubCategoryRepository
{
    public async Task<Guid> GetIdAsync(SubCategoryType subCategoryType)
    {
        var subCategory = await _dbSet
            .Where(subCategory => subCategory.Name == subCategoryType.GetStringValue())
            .FirstAsync();

        return subCategory.Id;
    }
}