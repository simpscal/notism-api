using Order.Domain.Common.Enums;
using Order.Domain.Common.Interfaces;

namespace Order.Domain.ProductSize;

public interface IProductSizeRepository : IRepository<ProductSize>
{
    public Task<ProductSize> GetAsync(SizeType sizeType);
    public Task<IEnumerable<ProductSize>> GetListAsync(IEnumerable<SizeType> sizeTypes);
}