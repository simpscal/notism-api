using Microsoft.EntityFrameworkCore;

using Notism.Domain.Product;
using Notism.Infrastructure.Common;

namespace Notism.Infrastructure.Products;

public class ProductRepository(AppDbContext appDbContext) :
    Repository<Product>(appDbContext),
    IProductRepository
{
    private readonly AppDbContext _appDbContext = appDbContext;

    public async Task<string> AddAsync(Product product)
    {
        _appDbContext.AttachRange(product.ProductColors);
        _appDbContext.AttachRange(product.ProductSizes);

        var result = await _dbSet.AddAsync(product);

        return result.Entity.Id.ToString();
    }

    public async Task<IEnumerable<Product>> GetAllAsync()
    {
        return await _dbSet.ToListAsync();
    }
}