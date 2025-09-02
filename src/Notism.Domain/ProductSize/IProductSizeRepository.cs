using Notism.Domain.Common.Enums;
using Notism.Domain.Common.Interfaces;

namespace Notism.Domain.ProductSize;

public interface IProductSizeRepository : IRepository<ProductSize>
{
    public Task<ProductSize> GetAsync(SizeType sizeType);
    public Task<IEnumerable<ProductSize>> GetListAsync(IEnumerable<SizeType> sizeTypes);
}