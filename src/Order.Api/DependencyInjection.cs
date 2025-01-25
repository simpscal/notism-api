using System.Text;

using Amazon.S3;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

using Order.Api.Services;
using Order.Shared.Interfaces;

namespace Order.Api;

public static class DependencyInjection
{
    public static IServiceCollection AddApi(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddMvc();
        services.AddControllers();
        services.AddEndpointsApiExplorer();

        services.AddSwaggerConfiguration();
        services.AddJwtAuthentication(configuration);

        services.AddProblemDetails();

        services.AddAWSS3();

        return services;
    }

    private static IServiceCollection AddSwaggerConfiguration(this IServiceCollection services)
    {
        services.AddSwaggerGen(options =>
        {
            options.AddSecurityDefinition(
                "Bearer",
                new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                });

            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" },
                    },
                    new string[] { }
                },
            });
        });

        return services;
    }

    private static IServiceCollection AddJwtAuthentication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
                        configuration["JwtSettings:Secret"] ??
                        throw new Exception("Empty JWTSettings Secret"))),
                };
            });

        return services;
    }

    private static IServiceCollection AddAWSS3(this IServiceCollection services)
    {
        services.AddSingleton<IFileStorageService, S3Service>();

        services.AddSingleton<IAmazonS3>(sp =>
        {
            var configuration = sp.GetRequiredService<IConfiguration>();

            return new AmazonS3Client(
                configuration["AWS:AccessKey"],
                configuration["AWS:SecretKey"],
                Amazon.RegionEndpoint.GetBySystemName(configuration["AWS:Region"]));
        });

        return services;
    }
}