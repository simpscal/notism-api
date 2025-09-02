using Microsoft.EntityFrameworkCore;

using Notism.Domain.Common.Enums;
using Notism.Domain.ProductColor;
using Notism.Infrastructure.Common;
using Notism.Shared.Extensions;

namespace Notism.Infrastructure.ProductColors;

public class ProductColorRepository(AppDbContext appDbContext)
    : Repository<ProductColor>(appDbContext), IProductColorRepository
{
    public async Task<ProductColor> GetAsync(ColorType colorType)
    {
        var productColor = await _dbSet
            .Where(productColor => productColor.Name == colorType.GetStringValue())
            .FirstAsync();

        return productColor;
    }

    public async Task<IEnumerable<ProductColor>> GetListAsync(IEnumerable<ColorType> colorTypes)
    {
        var colors = colorTypes.Select(c => c.GetStringValue());

        var productColors = await _dbSet
            .Where(productColor => colors.Contains(productColor.Name))
            .ToListAsync();

        return productColors;
    }
}