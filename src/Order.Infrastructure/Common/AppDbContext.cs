using Microsoft.EntityFrameworkCore;

using Order.Domain.Category;
using Order.Domain.Product;
using Order.Domain.ProductColor;
using Order.Domain.ProductSize;
using Order.Domain.SubCategory;
using Order.Domain.User;

namespace Order.Infrastructure.Common;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Product> Products { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<SubCategory> SubCategories { get; set; }
    public DbSet<ProductColor> ProductColors { get; set; }
    public DbSet<ProductSize> ProductSizes { get; set; }
    public DbSet<User> Users { get; set; }
}