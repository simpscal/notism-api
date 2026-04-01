using Notism.Domain.Food;
using Notism.Infrastructure.Persistence;

namespace Notism.Infrastructure.Repositories;

public class FoodRepository : Repository<Food>, IFoodRepository
{
    public FoodRepository(AppDbContext appDbContext)
        : base(appDbContext)
    {
    }
}