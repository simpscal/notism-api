using Notism.Api;
using Notism.Api.Endpoints;
using Notism.Api.Middlewares;
using Notism.Application;
using Notism.Infrastructure;

var builder = WebApplication.CreateBuilder(args);
{
    builder.Services
        .AddApi(builder.Configuration, builder.Environment)
        .AddApplication()
        .AddInfrastructure(builder.Configuration);
}

var app = builder.Build();
{
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
    app.MapStorageEndpoints();

    app.Run();
}