using Microsoft.EntityFrameworkCore;

using Notism.Domain.Category;
using Notism.Domain.Product;
using Notism.Domain.ProductColor;
using Notism.Domain.ProductSize;
using Notism.Domain.SubCategory;
using Notism.Domain.User;

namespace Notism.Infrastructure.Common;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Product> Products { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<SubCategory> SubCategories { get; set; }
    public DbSet<ProductColor> ProductColors { get; set; }
    public DbSet<ProductSize> ProductSizes { get; set; }
    public DbSet<User> Users { get; set; }
}