using Notism.Domain.Common.Enums;
using Notism.Domain.Common.Interfaces;

namespace Notism.Domain.ProductColor;

public interface IProductColorRepository : IRepository<ProductColor>
{
    public Task<ProductColor> GetAsync(ColorType colorType);
    public Task<IEnumerable<ProductColor>> GetListAsync(IEnumerable<ColorType> colorTypes);
}