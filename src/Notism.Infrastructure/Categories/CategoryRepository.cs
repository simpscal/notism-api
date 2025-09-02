using Microsoft.EntityFrameworkCore;

using Notism.Domain.Category;
using Notism.Domain.Common.Enums;
using Notism.Infrastructure.Common;
using Notism.Shared.Extensions;

namespace Notism.Infrastructure.Categories;

public class CategoryRepository(AppDbContext appDbContext) :
    Repository<Category>(appDbContext),
    ICategoryRepository
{
    public async Task<Guid> GetIdAsync(CategoryType categoryType)
    {
        var category = await _dbSet
            .Where(category => category.Name == categoryType.GetStringValue())
            .FirstAsync();

        return category.Id;
    }
}