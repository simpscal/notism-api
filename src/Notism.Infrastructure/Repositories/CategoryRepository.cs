using Notism.Domain.Food;
using Notism.Infrastructure.Persistence;

namespace Notism.Infrastructure.Repositories;

public class CategoryRepository : Repository<Category>, ICategoryRepository
{
    public CategoryRepository(AppDbContext appDbContext)
        : base(appDbContext)
    {
    }
}
