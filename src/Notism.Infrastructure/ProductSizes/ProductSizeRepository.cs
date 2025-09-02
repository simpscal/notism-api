using Microsoft.EntityFrameworkCore;

using Notism.Domain.Common.Enums;
using Notism.Domain.ProductSize;
using Notism.Infrastructure.Common;
using Notism.Shared.Extensions;

namespace Notism.Infrastructure.ProductSizes;

public class ProductSizeRepository(AppDbContext appDbContext) :
    Repository<ProductSize>(appDbContext),
    IProductSizeRepository
{
    public async Task<ProductSize> GetAsync(SizeType sizeType)
    {
        var productSize = await _dbSet
            .Where(s => s.Name == sizeType.GetStringValue())
            .FirstAsync();

        return productSize;
    }

    public async Task<IEnumerable<ProductSize>> GetListAsync(IEnumerable<SizeType> sizeTypes)
    {
        var sizes = sizeTypes.Select(sizeType => sizeType.GetStringValue());

        var productSizes = await _dbSet
            .Where(productSize => sizes.Contains(productSize.Name))
            .ToListAsync();

        return productSizes;
    }
}