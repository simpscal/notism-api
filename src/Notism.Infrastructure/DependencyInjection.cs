using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Notism.Domain.Category;
using Notism.Domain.Common.Interfaces;
using Notism.Domain.Product;
using Notism.Domain.ProductColor;
using Notism.Domain.ProductSize;
using Notism.Domain.SubCategory;
using Notism.Domain.User;
using Notism.Infrastructure.Categories;
using Notism.Infrastructure.Common;
using Notism.Infrastructure.ProductColors;
using Notism.Infrastructure.Products;
using Notism.Infrastructure.ProductSizes;
using Notism.Infrastructure.SubCategories;
using Notism.Infrastructure.Users;

namespace Notism.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(
            options =>
                options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")),
            ServiceLifetime.Transient);

        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<ISubCategoryRepository, SubCategoryRepository>();
        services.AddScoped<IProductSizeRepository, ProductSizeRepository>();
        services.AddScoped<IProductColorRepository, ProductColorRepository>();

        services.AddScoped<IUserRepository, UserRepository>();

        return services;
    }
}