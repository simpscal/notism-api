using System.Text;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

using Notism.Api.Interfaces;
using Notism.Api.Services;
using Notism.Shared.Configuration;
using Notism.Shared.Constants;

namespace Notism.Api;

public static class DependencyInjection
{
    public static IServiceCollection AddApi(this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment environment)
    {
        services.AddEndpointsApiExplorer();
        services.AddAuthorization();

        services.AddConfigurationOptions(configuration);
        services.AddSwaggerConfiguration();
        services.AddJwtAuthentication(configuration);
        services.AddCorsConfiguration();
        services.AddAntiforgeryConfiguration(environment);

        services.AddProblemDetails();

        services.AddScoped<ICookieService, CookieService>();

        return services;
    }

    private static IServiceCollection AddConfigurationOptions(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName));
        services.Configure<AwsSettings>(configuration.GetSection(AwsSettings.SectionName));
        services.Configure<ResendSettings>(configuration.GetSection(ResendSettings.SectionName));
        services.Configure<EmailSettings>(configuration.GetSection(EmailSettings.SectionName));
        services.Configure<ClientAppSettings>(configuration.GetSection(ClientAppSettings.SectionName));
        services.Configure<GoogleOAuthSettings>(configuration.GetSection(GoogleOAuthSettings.SectionName));

        return services;
    }

    private static IServiceCollection AddCorsConfiguration(this IServiceCollection services)
    {
        services.AddCors(options =>
        {
            options.AddPolicy("DevelopmentCorsPolicy", builder =>
            {
                builder
                    .WithOrigins("http://localhost:4200")
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .WithExposedHeaders(HeaderNames.AntiForgeryToken)
                    .AllowCredentials();
            });

            options.AddPolicy("ProductionCorsPolicy", builder =>
            {
                builder
                    .WithOrigins("https://localhost:3000", "https://yourdomain.com")
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .WithExposedHeaders(HeaderNames.AntiForgeryToken)
                    .AllowCredentials();
            });
        });

        return services;
    }

    private static IServiceCollection AddAntiforgeryConfiguration(this IServiceCollection services, IWebHostEnvironment environment)
    {
        services.AddAntiforgery(options =>
        {
            options.HeaderName = HeaderNames.AntiForgeryToken;
            options.Cookie.Name = CookieNames.AntiForgery;
            options.Cookie.HttpOnly = true;
            options.Cookie.SecurePolicy = environment.IsDevelopment()
                ? CookieSecurePolicy.None
                : CookieSecurePolicy.Always;

            options.Cookie.SameSite = environment.IsDevelopment()
                ? SameSiteMode.Lax
                : SameSiteMode.None;
        });

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
                    ClockSkew = TimeSpan.Zero,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
                        configuration["JwtSettings:Secret"] ??
                        throw new Exception("Empty JWTSettings Secret"))),
                };
            });

        return services;
    }
}