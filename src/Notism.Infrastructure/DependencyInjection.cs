using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Notism.Application.Common.Interfaces;
using Notism.Domain.User;
using Notism.Infrastructure.Common;
using Notism.Infrastructure.Services;
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

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IPasswordService, PasswordService>();

        return services;
    }
}