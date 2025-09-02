using Notism.Domain.Common.Enums;
using Notism.Domain.Common.Interfaces;

namespace Notism.Domain.Category;

public interface ICategoryRepository : IRepository<Category>
{
    public Task<Guid> GetIdAsync(CategoryType categoryType);
}