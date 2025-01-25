using Microsoft.EntityFrameworkCore;

using Order.Domain.Category;
using Order.Domain.Common.Enums;
using Order.Infrastructure.Common;
using Order.Shared.Extensions;

namespace Order.Infrastructure.Categories;

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