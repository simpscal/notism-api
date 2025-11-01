# Notism Worker Service

This project contains background workers for the Notism application.

## Architecture

### BaseWorker
All workers inherit from `BaseWorker` which provides:
- Common background service functionality
- Automatic service scope management
- Standardized logging
- Error handling patterns
- Configurable execution intervals

### Current Workers

#### TokenCleanupWorker
- **Purpose**: Cleans up expired refresh tokens and password reset tokens
- **Interval**: Configurable via `TokenCleanup:IntervalInHours` (default: 168 hours / 7 days)
- **Service**: Uses `ITokenCleanupService` for cleanup logic

### Adding New Workers

To add a new worker:

1. Create a new class inheriting from `BaseWorker`
2. Implement the required abstract members:
   - `WorkerName`: Display name for logging
   - `GetExecutionInterval()`: How often the worker should run
   - `ExecuteWorkAsync()`: The actual work to perform
3. Register the worker in `Program.cs` using `AddHostedService<YourWorker>()`

Example:
```csharp
public class MyCustomWorker : BaseWorker
{
    public MyCustomWorker(
        ILogger<MyCustomWorker> logger,
        IServiceScopeFactory serviceScopeFactory,
        IConfiguration configuration)
        : base(logger, serviceScopeFactory, configuration)
    {
    }

    protected override string WorkerName => "My Custom Worker";

    protected override TimeSpan GetExecutionInterval()
    {
        var interval = _configuration.GetValue<int>("MyWorker:IntervalInMinutes", 60);
        return TimeSpan.FromMinutes(interval);
    }

    protected override async Task ExecuteWorkAsync(IServiceScope scope, CancellationToken cancellationToken)
    {
        var myService = scope.ServiceProvider.GetRequiredService<IMyService>();
        await myService.DoWorkAsync(cancellationToken);
    }
}
```

Then register it in `Program.cs`:
```csharp
builder.Services.AddHostedService<MyCustomWorker>();
```

## Configuration

Workers can be configured via `appsettings.json`:

```json
{
  "TokenCleanup": {
    "IntervalInHours": 168
  },
  "EmailNotification": {
    "IntervalInMinutes": 30
  }
}
```

## Running

```bash
dotnet run --project src/Notism.Worker/Notism.Worker.csproj
```

## Dependencies

- **Notism.Infrastructure**: Database access and repositories
- **Microsoft.Extensions.Hosting**: Background service framework
- **Microsoft.Extensions.DependencyInjection**: Service scope management