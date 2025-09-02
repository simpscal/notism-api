using FluentValidation;
using FluentValidation.AspNetCore;

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

using Notism.Application.Common.Interceptors;
using Notism.Application.Common.Middlewares;
using Notism.Application.Common.Utilities;

namespace Notism.Application;

public static class DependencyInjection
{
    public static IApplicationBuilder UseApplication(this IApplicationBuilder app)
    {
        app.UseMiddleware<ExceptionValidatorMiddleware>();

        return app;
    }

    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddMediatR().AddValidations().AddMappers().AddUtilities();

        return services;
    }

    private static IServiceCollection AddMediatR(this IServiceCollection services)
    {
        services.AddMediatR(options =>
        {
            options.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly);
        });

        return services;
    }

    private static IServiceCollection AddValidations(this IServiceCollection services)
    {
        services.AddFluentValidationAutoValidation();
        services.AddValidatorsFromAssemblyContaining(typeof(DependencyInjection));
        services.AddSingleton<IValidatorInterceptor, ThrowingValidatorInterceptor>();

        return services;
    }

    private static IServiceCollection AddMappers(this IServiceCollection services)
    {
        services.AddAutoMapper(typeof(DependencyInjection).Assembly);

        return services;
    }

    private static IServiceCollection AddUtilities(this IServiceCollection services)
    {
        services.AddSingleton<TokenUtility>();

        return services;
    }
}