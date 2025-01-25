using Order.Domain.Common.Enums;
using Order.Domain.Common.Interfaces;

namespace Order.Domain.Category;

public interface ICategoryRepository : IRepository<Category>
{
    public Task<Guid> GetIdAsync(CategoryType categoryType);
}