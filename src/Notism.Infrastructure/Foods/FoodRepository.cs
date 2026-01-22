using Notism.Domain.Food;
using Notism.Infrastructure.Common;

namespace Notism.Infrastructure.Foods;

public class FoodRepository : Repository<Food>, IFoodRepository
{
    public FoodRepository(AppDbContext appDbContext)
        : base(appDbContext)
    {
    }
}