using Amazon.S3;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Notism.Application.Common.Interfaces;
using Notism.Domain.Common.Interfaces;
using Notism.Domain.RefreshToken;
using Notism.Domain.User;
using Notism.Infrastructure.Common;
using Notism.Infrastructure.RefreshTokens;
using Notism.Infrastructure.Services;
using Notism.Infrastructure.Users;

using Resend;

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

        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IPasswordService, PasswordService>();
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<IStorageService, S3StorageService>();

        services.AddHttpClient<IHttpService, HttpService>();
        services.AddScoped<IGoogleOAuthService, GoogleOAuthService>();

        services.AddAWSS3(configuration);
        services.AddResend(configuration);

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

    private static IServiceCollection AddResend(this IServiceCollection services, IConfiguration configuration)
    {
        var apiKey = configuration["Resend:ApiKey"] ?? throw new ArgumentNullException("Resend:ApiKey configuration is missing");

        services.AddOptions();
        services.Configure<ResendClientOptions>(o =>
        {
            o.ApiToken = apiKey;
        });
        services.AddHttpClient<ResendClient>();
        services.AddTransient<IResend, ResendClient>();

        return services;
    }
}