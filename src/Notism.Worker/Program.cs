using Notism.Infrastructure;
using Notism.Worker.Interfaces;
using Notism.Worker.Services;
using Notism.Worker.Workers;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddScoped<ITokenCleanupService, TokenCleanupService>();

builder.Services.AddHostedService<TokenCleanupWorker>();

var host = builder.Build();
host.Run();
