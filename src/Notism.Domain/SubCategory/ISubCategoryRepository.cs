using Notism.Domain.Common.Enums;
using Notism.Domain.Common.Interfaces;

namespace Notism.Domain.SubCategory;

public interface ISubCategoryRepository : IRepository<SubCategory>
{
    public Task<Guid> GetIdAsync(SubCategoryType subCategoryType);
}