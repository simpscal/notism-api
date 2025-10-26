# Notism API - Clean Architecture Implementation

## 📋 Table of Contents

1. [Architecture Overview](#architecture-overview)
2. [Layer Definitions](#layer-definitions)
3. [Dependency Rules](#dependency-rules)
4. [Implementation Guidelines](#implementation-guidelines)
5. [Folder Structure](#folder-structure)

---

## Architecture Overview

### The Clean Architecture Cone

```
┌─────────────────────────────────────────┐
│              API Layer                  │  ← Minimal APIs, Middleware, Endpoints
├─────────────────────────────────────────┤
│          Application Layer              │  ← Handlers (CQRS), Request/Response Models
├─────────────────────────────────────────┤
│            Domain Layer                 │  ← Aggregates, Entities, Business Rules
├─────────────────────────────────────────┤
│        Infrastructure Layer             │  ← Data Access, External Services
├─────────────────────────────────────────┤
│            Shared Layer                 │  ← Common Utilities, Extensions
└─────────────────────────────────────────┘
```

### Dependency Flow Direction

```
API → Application → Domain ← Infrastructure
 ↓        ↓          ↑           ↑
Endpoints → Handlers → Business Logic ← Data Access
 ↓        ↓          ↑           ↑
 └────────┴──────────┴───────────┴─────── Shared (Referenced by all)
```

---

## Layer Definitions

### 🎯 Domain Layer (Core)

**Purpose**: Contains business logic organized as aggregates that represent cohesive business concepts and maintain consistency boundaries.

#### Responsibilities

- **Aggregate Roots** - Entry points to aggregate boundaries with business behavior
- **Entities** - Objects with identity within aggregates
- **Value Objects** - Immutable objects representing domain concepts
- **Domain Events** - Represent important business occurrences within aggregates
- **Domain Services** - Business logic that doesn't belong to a single aggregate
- **Business Rules** - Invariants and constraints enforced within aggregate boundaries
- **Repository Interfaces** - Contracts for persisting aggregates

#### What Belongs Here

```csharp
// ✅ Aggregate Root
public class Order : BaseEntity, IAggregateRoot
{
    private readonly List<OrderItem> _items = new();
    private readonly List<DomainEvent> _domainEvents = new();

    public string OrderNumber { get; private set; }
    public Guid CustomerId { get; private set; }
    public OrderStatus Status { get; private set; }
    public Money Total { get; private set; }
    
    public IReadOnlyList<OrderItem> Items => _items.AsReadOnly();
    public IReadOnlyList<DomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    public void AddItem(Product product, int quantity)
    {
        if (Status != OrderStatus.Draft)
            throw new InvalidOperationException("Cannot modify confirmed order");

        var existingItem = _items.FirstOrDefault(i => i.ProductId == product.Id);
        if (existingItem != null)
        {
            existingItem.UpdateQuantity(existingItem.Quantity + quantity);
        }
        else
        {
            var orderItem = new OrderItem(product.Id, product.Name, product.Price, quantity);
            _items.Add(orderItem);
        }

        RecalculateTotal();
        AddDomainEvent(new OrderItemAddedEvent(Id, product.Id, quantity));
    }

    public void Confirm()
    {
        if (!_items.Any())
            throw new InvalidOperationException("Cannot confirm empty order");

        Status = OrderStatus.Confirmed;
        AddDomainEvent(new OrderConfirmedEvent(Id, OrderNumber, CustomerId, Total));
    }

    private void RecalculateTotal()
    {
        Total = _items.Aggregate(new Money(0, "USD"), (sum, item) => sum.Add(item.LineTotal));
    }
}

// ✅ Repository Interface (Aggregate-focused)
public interface IOrderRepository
{
    Task<Order> GetByIdAsync(Guid id);
    Task<Order> GetByOrderNumberAsync(string orderNumber);
    Task<IEnumerable<Order>> GetByCustomerIdAsync(Guid customerId);
    void Add(Order order);
    void Update(Order order);
    Task SaveChangesAsync();
}
```

---

### 🔧 Application Layer (Command/Query Handlers)

**Purpose**: Orchestrates domain operations through CQRS handlers that process commands and queries using MediatR, providing a clear separation between read and write operations.

#### Responsibilities

- **Command Handlers** - Process write operations (Create, Update, Delete)
- **Query Handlers** - Process read operations (Get, Search, List)
- **Request/Response Models** - Input and output models for handlers
- **Validation** - Input validation using FluentValidation
- **Business Orchestration** - Coordinate between multiple domain aggregates
- **Exception Handling** - Throw ResultFailureException for business rule violations
- **Domain Event Handling** - React to domain events for cross-aggregate operations

#### What Belongs Here

```csharp
// ✅ Command Handler - Write operations
public class RegisterHandler : IRequestHandler<RegisterRequest, Result<RegisterResponse>>
{
    private readonly IUserRepository _userRepository;
    private readonly ITokenService _tokenService;
    private readonly IPasswordService _passwordService;
    private readonly IMapper _mapper;

    public RegisterHandler(
        IUserRepository userRepository,
        ITokenService tokenService,
        IPasswordService passwordService,
        IMapper mapper)
    {
        _userRepository = userRepository;
        _tokenService = tokenService;
        _passwordService = passwordService;
        _mapper = mapper;
    }

    public async Task<Result<RegisterResponse>> Handle(RegisterRequest request, CancellationToken cancellationToken)
    {
        // 1. Check business rules
        var existingUser = await _userRepository.FindByExpressionAsync(new UserByEmailSpecification(request.Email));
        if (existingUser != null)
        {
            throw new ResultFailureException("User with this email already exists");
        }

        // 2. Create domain entity
        var hashedPassword = _passwordService.HashPassword(request.Password);
        var user = Domain.User.User.Create(request.Email, hashedPassword);

        // 3. Persist changes
        await _userRepository.AddAsync(user);
        await _userRepository.SaveChangesAsync();

        // 4. Generate response
        var tokenResult = await _tokenService.GenerateTokenAsync(user);
        var response = _mapper.Map<RegisterResponse>(user);
        response.Token = tokenResult.Token;
        response.ExpiresAt = tokenResult.ExpiresAt;

        return Result<RegisterResponse>.Success(response);
    }
}

// ✅ Request/Response Models
public class UpdateUserProfileRequest : IRequest<Result<UpdateUserProfileResponse>>
{
    public Guid UserId { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Email { get; set; }
    public string? Role { get; set; } // "user" or "admin"
}

public class UpdateUserProfileResponse
{
    public Guid UserId { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string Email { get; set; }
    public string Role { get; set; }
    public string Message { get; set; }
}

// ✅ Generic Enum Converter - Reusable utility
public static class EnumConverter
{
    public static TEnum? FromString<TEnum>(string? value) 
        where TEnum : struct, Enum
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        return Enum.TryParse<TEnum>(value, ignoreCase: true, out var result) ? result : null;
    }

    public static string ToCamelCase<TEnum>(TEnum enumValue) 
        where TEnum : struct, Enum
    {
        var enumString = enumValue.ToString();
        return char.ToLowerInvariant(enumString[0]) + enumString[1..];
    }

    public static bool IsValidEnumString<TEnum>(string? value) 
        where TEnum : struct, Enum
    {
        if (string.IsNullOrWhiteSpace(value))
            return true;

        return Enum.TryParse<TEnum>(value, ignoreCase: true, out _);
    }

    public static IEnumerable<string> GetValidEnumStrings<TEnum>() 
        where TEnum : struct, Enum
    {
        return Enum.GetValues<TEnum>().Select(ToCamelCase);
    }
}

// ✅ Validation using FluentValidation
public class UpdateUserProfileValidator : AbstractValidator<UpdateUserProfileRequest>
{
    public UpdateUserProfileValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("UserId is required");

        RuleFor(x => x.FirstName)
            .MaximumLength(50)
            .WithMessage("First name cannot exceed 50 characters");

        RuleFor(x => x.LastName)
            .MaximumLength(50)
            .WithMessage("Last name cannot exceed 50 characters");

        RuleFor(x => x.Email)
            .EmailAddress()
            .When(x => !string.IsNullOrWhiteSpace(x.Email))
            .WithMessage("Email must be a valid email address");

        RuleFor(x => x.Role)
            .Must(role => EnumConverter.IsValidEnumString<UserRole>(role))
            .WithMessage($"Role must be either '{string.Join("' or '", EnumConverter.GetValidEnumStrings<UserRole>())}'");
    }
}
```

---

### 🌐 API Layer (Minimal APIs)

**Purpose**: Handles HTTP communication using Minimal APIs with feature-based endpoint organization, delegating request processing to MediatR handlers and managing cross-cutting concerns through middleware.

#### Responsibilities

- **Minimal API Endpoints** - Feature-based HTTP endpoints using MediatR
- **Authentication & Authorization** - JWT-based auth with role-based access control
- **Error Handling** - Global exception handling via middleware
- **Request Validation** - Automatic validation through FluentValidation pipeline
- **API Documentation** - OpenAPI/Swagger integration
- **CORS Configuration** - Cross-origin resource sharing setup

#### What Belongs Here

```csharp
// ✅ Feature-based Minimal API Endpoints
public static class UserEndpoints
{
    public static void MapUserEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var users = endpoints.MapGroup("/api/users")
            .WithTags("Users")
            .WithOpenApi();

        // Admin-only endpoint using custom authorization attribute
        users.MapPut("/{userId:guid}/profile", [RequireAdmin] async (
            Guid userId,
            UpdateUserProfileRequest request,
            ISender sender) =>
        {
            request.UserId = userId;
            var result = await sender.Send(request);
            
            return result.IsSuccess 
                ? Results.Ok(result.Value)
                : Results.BadRequest(new { message = result.Error });
        })
        .WithName("UpdateUserProfile")
        .WithSummary("Update user profile (Admin only)")
        .Produces<UpdateUserProfileResponse>(200)
        .Produces<object>(400);
    }
}

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var auth = endpoints.MapGroup("/api/auth")
            .WithTags("Authentication")
            .WithOpenApi();

        auth.MapPost("/login", async (
            LoginRequest request,
            ISender sender) =>
        {
            var result = await sender.Send(request);
            return result.IsSuccess 
                ? Results.Ok(result.Value)
                : Results.BadRequest(new { message = result.Error });
        })
        .WithName("Login")
        .WithSummary("User authentication")
        .Produces<LoginResponse>(200)
        .Produces<object>(400);

        auth.MapPost("/register", async (
            RegisterRequest request,
            ISender sender) =>
        {
            var result = await sender.Send(request);
            return result.IsSuccess
                ? Results.Created($"/api/users/{result.Value.UserId}", result.Value)
                : Results.BadRequest(new { message = result.Error });
        })
        .WithName("Register")
        .WithSummary("User registration")
        .Produces<RegisterResponse>(201)
        .Produces<object>(400);

        auth.MapPost("/refresh-token", async (
            RefreshTokenRequest request,
            ISender sender) =>
        {
            var result = await sender.Send(request);
            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.BadRequest(new { message = result.Error });
        });

        auth.MapPost("/request-password-reset", async (
            RequestPasswordResetRequest request,
            ISender sender) =>
        {
            var result = await sender.Send(request);
            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.BadRequest(new { message = result.Error });
        });

        auth.MapPost("/reset-password", async (
            ResetPasswordRequest request,
            ISender sender) =>
        {
            var result = await sender.Send(request);
            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.BadRequest(new { message = result.Error });
        });
    }
}

// ✅ Custom Authorization Attribute
public class RequireAdminAttribute : AuthorizeAttribute
{
    public RequireAdminAttribute()
    {
        Roles = "admin"; // Matches JWT role claim in camelCase
    }
}

// ✅ Result Failure Middleware
public class ResultFailureMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ResultFailureMiddleware> _logger;

    public ResultFailureMiddleware(RequestDelegate next, ILogger<ResultFailureMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (ResultFailureException ex)
        {
            await HandleResultFailureAsync(context, ex);
        }
    }

    private async Task HandleResultFailureAsync(HttpContext context, ResultFailureException exception)
    {
        _logger.LogWarning("Business rule violation: {Message}", exception.Message);

        context.Response.StatusCode = 400;
        context.Response.ContentType = "application/json";

        var response = new
        {
            message = exception.Message,
            timestamp = DateTime.UtcNow
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }
}

// ✅ Program.cs Configuration
var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));
builder.Services.AddAutoMapper(typeof(AuthMappingProfile));
builder.Services.AddAuthentication(/* JWT configuration */);
builder.Services.AddAuthorization();

var app = builder.Build();

// Configure pipeline
app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<ResultFailureMiddleware>();

// Map endpoints
app.MapAuthEndpoints();
app.MapUserEndpoints();

app.Run();
```

---

### 🏗️ Infrastructure Layer (External Concerns)

**Purpose**: Implements interfaces defined by inner layers and provides aggregate persistence, handling aggregate boundaries correctly while managing external dependencies.

#### Responsibilities

- **Aggregate Repositories** - Persist and retrieve complete aggregates
- **External Services** - HTTP clients, message queues, file systems
- **Event Dispatching** - Publish domain events after aggregate persistence
- **Database Context** - Manage aggregate configurations and relationships
- **Caching** - Cache complete aggregates when appropriate
- **Configuration** - Settings, environment variables

#### What Belongs Here

```csharp
// ✅ Aggregate Repository Implementation
public class OrderRepository : IOrderRepository
{
    private readonly ApplicationDbContext _context;

    public OrderRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Order> GetByIdAsync(Guid id)
    {
        // Load complete aggregate
        return await _context.Orders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == id);
    }

    public async Task<IEnumerable<Order>> GetByCustomerIdAsync(Guid customerId)
    {
        return await _context.Orders
            .Include(o => o.Items)
            .Where(o => o.CustomerId == customerId)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();
    }

    public void Add(Order order)
    {
        _context.Orders.Add(order);
    }

    public void Update(Order order)
    {
        _context.Orders.Update(order);
    }

    public async Task SaveChangesAsync()
    {
        // Domain events are handled in DbContext.SaveChangesAsync override
        await _context.SaveChangesAsync();
    }
}

// ✅ Database Context with Event Handling
public class ApplicationDbContext : DbContext
{
    private readonly IMediator _mediator;

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, IMediator mediator)
        : base(options)
    {
        _mediator = mediator;
    }

    public DbSet<Order> Orders { get; set; }
    public DbSet<Customer> Customers { get; set; }
    public DbSet<Product> Products { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new OrderConfiguration());
        modelBuilder.ApplyConfiguration(new CustomerConfiguration());
        modelBuilder.ApplyConfiguration(new ProductConfiguration());

        base.OnModelCreating(modelBuilder);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Collect domain events from aggregates
        var aggregatesWithEvents = ChangeTracker.Entries<IAggregateRoot>()
            .Where(e => e.Entity.DomainEvents.Any())
            .Select(e => e.Entity)
            .ToList();

        var domainEvents = aggregatesWithEvents
            .SelectMany(a => a.DomainEvents)
            .ToList();

        // Clear events before saving
        aggregatesWithEvents.ForEach(a => a.ClearDomainEvents());

        var result = await base.SaveChangesAsync(cancellationToken);

        // Dispatch events after successful save
        foreach (var domainEvent in domainEvents)
        {
            await _mediator.Publish(domainEvent, cancellationToken);
        }

        return result;
    }
}

// ✅ External Service Implementation
public class EmailService : IEmailService
{
    public async Task SendOrderConfirmationAsync(Guid orderId)
    {
        // Implementation using SendGrid, AWS SES, etc.
    }
}
```

---

### 🔧 Shared Layer (Common Utilities)

**Purpose**: Provides common utilities, extensions, constants, and helper classes that can be used across all layers without creating dependencies between business layers.

#### Responsibilities

- **Extension Methods** - Common extensions for built-in types
- **Constants** - Application-wide constants
- **Utilities** - Helper classes and static methods
- **Common Models** - Generic result types, pagination models
- **Validation Helpers** - Reusable validation logic
- **Enumerations** - Common enums used across layers

#### What Belongs Here

```csharp
// ✅ Result Pattern - Used across all layers
public class Result
{
    public bool IsSuccess { get; protected set; }
    public string Error { get; protected set; }

    protected Result(bool isSuccess, string error)
    {
        IsSuccess = isSuccess;
        Error = error;
    }

    public static Result Success() => new(true, string.Empty);
    public static Result Failure(string error) => new(false, error);
}

public class Result<T> : Result
{
    public T Value { get; private set; }

    private Result(bool isSuccess, T value, string error) : base(isSuccess, error)
    {
        Value = value;
    }

    public static Result<T> Success(T value) => new(true, value, string.Empty);
    public static new Result<T> Failure(string error) => new(false, default, error);
}

// ✅ Pagination Models
public class PagedResult<T>
{
    public IEnumerable<T> Items { get; set; }
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    public bool HasPrevious => PageNumber > 1;
    public bool HasNext => PageNumber < TotalPages;
}

public class PagedQuery
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string SearchTerm { get; set; }
    public string SortBy { get; set; }
    public bool SortDescending { get; set; }
}

// ✅ Extension Methods
public static class StringExtensions
{
    public static bool IsValidEmail(this string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        try
        {
            var emailAddress = new System.Net.Mail.MailAddress(email);
            return emailAddress.Address == email;
        }
        catch
        {
            return false;
        }
    }

    public static string ToTitleCase(this string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return string.Empty;

        return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(input.ToLower());
    }
}

public static class EnumerableExtensions
{
    public static bool IsNullOrEmpty<T>(this IEnumerable<T> source)
    {
        return source == null || !source.Any();
    }

    public static PagedResult<T> ToPagedResult<T>(this IEnumerable<T> source, int pageNumber, int pageSize)
    {
        var totalCount = source.Count();
        var items = source.Skip((pageNumber - 1) * pageSize).Take(pageSize);

        return new PagedResult<T>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }
}

public static class DateTimeExtensions
{
    public static bool IsWeekend(this DateTime date)
    {
        return date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday;
    }

    public static DateTime StartOfDay(this DateTime date)
    {
        return new DateTime(date.Year, date.Month, date.Day, 0, 0, 0, 0);
    }

    public static DateTime EndOfDay(this DateTime date)
    {
        return new DateTime(date.Year, date.Month, date.Day, 23, 59, 59, 999);
    }
}

// ✅ Constants
public static class ApplicationConstants
{
    public const int DEFAULT_PAGE_SIZE = 10;
    public const int MAX_PAGE_SIZE = 100;
    public const string DEFAULT_CURRENCY = "USD";
    public const int PASSWORD_MIN_LENGTH = 8;
}

public static class ConfigurationKeys
{
    public const string DATABASE_CONNECTION = "ConnectionStrings:DefaultConnection";
    public const string JWT_SECRET = "Authentication:JwtSecret";
    public const string EMAIL_API_KEY = "EmailService:ApiKey";
    public const string CACHE_CONNECTION = "ConnectionStrings:CacheConnection";
}

// ✅ Validation Helpers
public static class ValidationHelpers
{
    public static bool IsValidGuid(string value)
    {
        return Guid.TryParse(value, out _);
    }

    public static bool IsPositiveNumber(decimal value)
    {
        return value > 0;
    }

    public static bool IsValidPhoneNumber(string phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
            return false;

        // Basic phone number validation
        var phoneRegex = new Regex(@"^\+?[\d\s\-\(\)]{10,}$");
        return phoneRegex.IsMatch(phoneNumber);
    }
}

// ✅ Common Enumerations
public enum SortDirection
{
    Ascending,
    Descending
}

public enum OperationResult
{
    Success,
    NotFound,
    ValidationError,
    UnauthorizedAccess,
    InternalError
}
```

---

## Folder Structure

### Notism API - Current Project Structure

```
notism-api/
├── .dockerignore                                   # Docker ignore patterns
├── .editorconfig                                   # Editor configuration
├── .gitignore                                      # Git ignore patterns
├── .vscode/                                        # VS Code settings
├── Directory.Build.props                           # Global build properties
├── Directory.Packages.props                        # Central package management
├── Dockerfile                                      # API container definition
├── Notism.sln                                      # Solution file
├── README.md                                       # Project documentation
├── docker-compose.yml                              # Container orchestration
├── nginx.conf                                      # Reverse proxy configuration
│
├── docs/
│   └── architecture.md                            # This document
│
├── src/
│   ├── Notism.Shared/                             # 🔧 Shared Layer
│   │   ├── Attributes/
│   │   ├── Constants/
│   │   ├── Exceptions/
│   │   │   └── ResultFailureException.cs
│   │   ├── Extensions/
│   │   ├── Models/
│   │   │   ├── Pagination.cs
│   │   │   └── Result.cs
│   │   └── Notism.Shared.csproj
│   │
│   ├── Notism.Domain/                             # 🎯 Domain Layer
│   │   ├── Common/
│   │   │   ├── AggregateRoot.cs
│   │   │   ├── DomainEvent.cs
│   │   │   ├── Entity.cs
│   │   │   ├── ValueObject.cs
│   │   │   ├── Enums/
│   │   │   ├── Interfaces/
│   │   │   │   ├── IDomainEvent.cs
│   │   │   │   ├── IRepository.cs
│   │   │   │   ├── ISpecification.cs
│   │   │   │   └── IUnitOfWork.cs
│   │   │   └── Specifications/
│   │   │
│   │   ├── User/                                  # User Aggregate
│   │   │   ├── User.cs                            # Aggregate Root
│   │   │   ├── IUserRepository.cs
│   │   │   ├── PasswordResetToken.cs              # Entity
│   │   │   ├── Enums/
│   │   │   │   └── UserRole.cs
│   │   │   ├── Events/
│   │   │   │   ├── PasswordResetCompletedEvent.cs
│   │   │   │   ├── PasswordResetRequestedEvent.cs
│   │   │   │   ├── UserCreatedEvent.cs
│   │   │   │   ├── UserPasswordChangedEvent.cs
│   │   │   │   └── UserProfileUpdatedEvent.cs
│   │   │   ├── Specifications/
│   │   │   │   ├── ActivePasswordResetTokenByUserIdSpecification.cs
│   │   │   │   ├── PasswordResetTokenByTokenSpecification.cs
│   │   │   │   ├── UserByEmailSpecification.cs
│   │   │   │   └── UserByIdSpecification.cs
│   │   │   └── ValueObjects/
│   │   │       ├── Email.cs
│   │   │       └── Password.cs
│   │   │
│   │   ├── RefreshToken/                          # RefreshToken Aggregate
│   │   │   ├── RefreshToken.cs
│   │   │   ├── IRefreshTokenRepository.cs
│   │   │   └── Specifications/
│   │   │       ├── RefreshTokenByTokenSpecification.cs
│   │   │       └── RefreshTokenByUserIdSpecification.cs
│   │   │
│   │   └── Notism.Domain.csproj
│   │
│   ├── Notism.Application/                        # 🔧 Application Layer
│   │   ├── Common/
│   │   │   ├── Behaviors/
│   │   │   │   └── ValidationBehavior.cs
│   │   │   ├── Constants/
│   │   │   ├── Interfaces/
│   │   │   │   ├── IEmailService.cs
│   │   │   │   ├── IPasswordService.cs
│   │   │   │   └── ITokenService.cs
│   │   │   ├── Mappings/
│   │   │   │   └── MappingProfile.cs
│   │   │   └── Utilities/
│   │   │       └── EnumConverter.cs               # Generic enum converter
│   │   │
│   │   ├── Auth/                                  # Authentication Features
│   │   │   ├── AuthMappingProfile.cs
│   │   │   ├── Login/
│   │   │   │   ├── LoginHandler.cs                # CQRS Handler
│   │   │   │   ├── LoginRequest.cs
│   │   │   │   ├── LoginRequestValidator.cs
│   │   │   │   └── LoginResponse.cs
│   │   │   ├── Register/
│   │   │   │   ├── RegisterHandler.cs
│   │   │   │   ├── RegisterRequest.cs
│   │   │   │   ├── RegisterRequestValidator.cs
│   │   │   │   └── RegisterResponse.cs
│   │   │   ├── RefreshToken/
│   │   │   │   ├── RefreshTokenHandler.cs
│   │   │   │   └── RefreshTokenRequest.cs
│   │   │   ├── RequestPasswordReset/
│   │   │   │   ├── RequestPasswordResetHandler.cs
│   │   │   │   ├── RequestPasswordResetRequest.cs
│   │   │   │   ├── RequestPasswordResetRequestValidator.cs
│   │   │   │   └── RequestPasswordResetResponse.cs
│   │   │   └── ResetPassword/
│   │   │       ├── ResetPasswordHandler.cs
│   │   │       ├── ResetPasswordRequest.cs
│   │   │       ├── ResetPasswordRequestValidator.cs
│   │   │       └── ResetPasswordResponse.cs
│   │   │
│   │   ├── User/                                  # User Management Features
│   │   │   └── UpdateProfile/
│   │   │       ├── UpdateUserProfileHandler.cs
│   │   │       ├── UpdateUserProfileRequest.cs
│   │   │       ├── UpdateUserProfileRequestValidator.cs
│   │   │       └── UpdateUserProfileResponse.cs
│   │   │
│   │   ├── DependencyInjection.cs
│   │   └── Notism.Application.csproj
│   │
│   ├── Notism.Infrastructure/                     # 🏗️ Infrastructure Layer
│   │   ├── Common/
│   │   │   ├── AppDbContext.cs
│   │   │   ├── Repository.cs                      # Generic repository
│   │   │   └── UnitOfWork.cs
│   │   │
│   │   ├── Migrations/                            # EF Core migrations
│   │   │   ├── 20241025123456_InitialCreate.cs
│   │   │   ├── 20241025234567_AddPasswordResetToken.cs
│   │   │   └── 20241026034538_ConvertUserRoleToString.cs
│   │   │
│   │   ├── RefreshTokens/
│   │   │   └── RefreshTokenRepository.cs
│   │   │
│   │   ├── Services/
│   │   │   ├── EmailService.cs
│   │   │   ├── PasswordService.cs
│   │   │   └── TokenService.cs                    # JWT generation with role claims
│   │   │
│   │   ├── Users/
│   │   │   └── UserRepository.cs
│   │   │
│   │   ├── DependencyInjection.cs
│   │   └── Notism.Infrastructure.csproj
│   │
│   └── Notism.Api/                                # 🌐 API Layer
│       ├── .env.development                       # Development environment variables
│       ├── .env.production                        # Production environment variables
│       │
│       ├── Attributes/
│       │   └── RequireAdminAttribute.cs           # Role-based authorization
│       │
│       ├── Endpoints/
│       │   ├── AuthEndpoints.cs                   # Authentication endpoints
│       │   └── UserEndpoints.cs                   # User management endpoints
│       │
│       ├── Middlewares/
│       │   └── ResultFailureMiddleware.cs         # Exception handling
│       │
│       ├── Models/
│       │   └── ErrorResponse.cs
│       │
│       ├── Properties/
│       │   └── launchSettings.json
│       │
│       ├── appsettings.json
│       ├── appsettings.Development.json
│       ├── appsettings.Production.json
│       ├── DependencyInjection.cs
│       ├── Program.cs                             # Minimal API configuration
│       └── Notism.Api.csproj
│
└── tests/                                         # 🧪 Test Projects
    ├── Notism.Application.Tests/
    │   ├── Auth/
    │   │   ├── Login/
    │   │   ├── Register/
    │   │   └── RefreshToken/
    │   ├── BaseTest.cs
    │   └── Notism.Application.Tests.csproj
    │
    └── Notism.Domain.Tests/                       # Comprehensive domain testing (122 tests)
        ├── RefreshToken/
        │   └── RefreshTokenTests.cs
        ├── User/
        │   ├── UserTests.cs                        # User aggregate tests
        │   ├── EmailTests.cs                       # Email value object tests
        │   ├── PasswordTests.cs                    # Password value object tests
        │   └── UserSpecificationTests.cs           # Specification pattern tests
        └── Notism.Domain.Tests.csproj
```

### Key Project Structure Highlights

#### 🎯 **Domain-Driven Design Organization**
- **Aggregates**: User and RefreshToken with clear boundaries
- **Value Objects**: Email and Password with business validation
- **Domain Events**: Comprehensive event handling for business actions
- **Specifications**: Encapsulated query logic for all data access

#### 🔧 **CQRS Implementation**
- **Handlers**: Separate command and query handlers for each feature
- **Validation**: FluentValidation validators for each request
- **Responses**: Dedicated response models for each operation
- **Feature Folders**: Authentication and User management organized by business capability

#### 🏗️ **Infrastructure Patterns**
- **Repository Pattern**: One repository per aggregate root
- **Unit of Work**: Transaction management and change tracking
- **Services**: External concerns (Email, Password hashing, JWT tokens)
- **Migrations**: Version-controlled database schema changes

#### 🌐 **API Design**
- **Minimal APIs**: Feature-based endpoint organization
- **Middleware**: Cross-cutting concerns (error handling, logging)
- **Authorization**: Role-based access control with custom attributes
- **Configuration**: Environment-specific settings management

#### 🧪 **Testing Strategy**
- **Domain Tests**: 122+ tests covering business logic and rules
- **Application Tests**: Handler validation and business flow testing
- **Separation**: Clear test organization matching source structure

This structure demonstrates clean architecture principles with proper separation of concerns, dependency inversion, and comprehensive testing coverage.