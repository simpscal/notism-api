using Order.Domain.Common.Enums;
using Order.Domain.Common.Interfaces;

namespace Order.Domain.SubCategory;

public interface ISubCategoryRepository : IRepository<SubCategory>
{
    public Task<Guid> GetIdAsync(SubCategoryType subCategoryType);
}