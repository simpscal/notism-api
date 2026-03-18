using System.Net.Http.Headers;

using Amazon.S3;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Notism.Application.Common.Interfaces;
using Notism.Domain.Cart;
using Notism.Domain.Common.Interfaces;
using Notism.Domain.Food;
using Notism.Domain.Order;
using Notism.Domain.RefreshToken;
using Notism.Domain.User;
using Notism.Infrastructure.Persistence;
using Notism.Infrastructure.Repositories;
using Notism.Infrastructure.Services;
using Notism.Shared.Configuration;

namespace Notism.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(
            options =>
                options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")),
            ServiceLifetime.Scoped);

        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
        services.AddScoped<IPasswordResetTokenRepository, PasswordResetTokenRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<IFoodRepository, FoodRepository>();
        services.AddScoped<ICartItemRepository, CartItemRepository>();
        services.AddScoped<IOrderRepository, OrderRepository>();

        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IPasswordService, PasswordService>();
        services.AddScoped<IStorageService, S3StorageService>();

        services.AddHttpClient<IHttpService, HttpService>();
        services.AddScoped<IGoogleOAuthService, GoogleOAuthService>();

        services.AddAWSS3(configuration);
        services.AddMailerSend(configuration);

        return services;
    }

    private static IServiceCollection AddAWSS3(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<IAmazonS3>(sp =>
        {
            return new AmazonS3Client(
                configuration["AWS:AccessKey"],
                configuration["AWS:SecretKey"],
                Amazon.RegionEndpoint.GetBySystemName(configuration["AWS:Region"]));
        });

        return services;
    }

    private static IServiceCollection AddMailerSend(this IServiceCollection services, IConfiguration configuration)
    {
        var apiKey = configuration[$"{EmailSettings.SectionName}:ApiKey"]
            ?? throw new ArgumentNullException("Email:ApiKey", "MailerSend API key configuration is missing");

        services.AddHttpClient<IEmailService, EmailService>(client =>
        {
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        });

        return services;
    }
}