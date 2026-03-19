using DotNetEnv;

using Microsoft.AspNetCore.HttpOverrides;

using Notism.Api;
using Notism.Api.Endpoints;
using Notism.Api.Middlewares;
using Notism.Application;
using Notism.Infrastructure;

var envFile = ".env";
if (File.Exists(envFile))
{
    Env.Load(envFile);
}

var builder = WebApplication.CreateBuilder(args);
{
    builder.Services
        .AddApi(builder.Configuration, builder.Environment)
        .AddApplication()
        .AddInfrastructure(builder.Configuration);
}

var app = builder.Build();
{
    var forwardedHeadersOptions = new ForwardedHeadersOptions
    {
        ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto,
    };
    forwardedHeadersOptions.KnownNetworks.Clear();
    forwardedHeadersOptions.KnownProxies.Clear();
    app.UseForwardedHeaders(forwardedHeadersOptions);

    app.UseCors(app.Environment.IsDevelopment() ? "DevelopmentCorsPolicy" : "ProductionCorsPolicy");

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseHttpsRedirection();

    app.UseAuthentication();
    app.UseAuthorization();

    app.UseAntiforgery();

    app.UseMiddleware<ResultFailureMiddleware>();

    app.MapAuthEndpoints();
    app.MapUserEndpoints();
    app.MapAdminEndpoints();
    app.MapStorageEndpoints();
    app.MapFoodEndpoints();
    app.MapCartEndpoints();
    app.MapOrderEndpoints();

    app.Run();
}