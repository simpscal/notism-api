using Notism.Domain.Common.Interfaces;

namespace Notism.Domain.Product;

public interface IProductRepository : IRepository<Product>
{
    public Task<string> AddAsync(Product product);
    public Task<IEnumerable<Product>> GetAllAsync();
}