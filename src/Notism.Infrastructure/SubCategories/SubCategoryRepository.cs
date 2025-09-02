using Microsoft.EntityFrameworkCore;

using Notism.Domain.Common.Enums;
using Notism.Domain.SubCategory;
using Notism.Infrastructure.Common;
using Notism.Shared.Extensions;

namespace Notism.Infrastructure.SubCategories;

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