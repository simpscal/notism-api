# Clean Architecture - 5 Layer Implementation Guide

## 📋 Table of Contents

1. [Architecture Overview](#architecture-overview)
2. [Layer Definitions](#layer-definitions)
3. [Dependency Rules](#dependency-rules)
4. [Implementation Guidelines](#implementation-guidelines)
5. [Folder Structure](#folder-structure)
6. [Design Patterns](#design-patterns)

---

## Architecture Overview

### The Clean Architecture Cone

```
┌─────────────────────────────────────────┐
│              API Layer                  │  ← Minimal APIs, Middleware, Endpoints
├─────────────────────────────────────────┤
│          Application Layer              │  ← Use Cases, Event Handlers
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
External → Use Cases → Business Logic ← Technical Implementation
 ↓        ↓          ↑           ↑
 └────────┴──────────┴───────────┴─────── Shared (Referenced by all)
```

### Key Characteristics

- **Inner layers define abstractions**, outer layers provide implementations
- **Dependencies point inward** - no inner layer knows about outer layers
- **Business rules are protected** from external changes
- **Framework-agnostic core** enables technology evolution
- **Aggregate pattern** ensures consistency boundaries and business rule enforcement
- **Use Cases** represent single business operations with focused responsibilities
- **Shared layer** provides common utilities accessible to all layers
- **AutoMapper** handles object-to-object mapping across layer boundaries

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

### 🔧 Application Layer (Use Cases)

**Purpose**: Orchestrates domain aggregates through focused use cases that represent single business operations, handling cross-aggregate coordination and mapping between domain models and DTOs.

#### Responsibilities

- **Use Cases** - Single-purpose business operations with focused responsibilities
- **Event Handlers** - Handle domain events from aggregates for cross-aggregate coordination
- **DTOs** - Data transfer objects for communication with outer layers
- **Commands/Queries** - Input models for use cases
- **Validation** - Input validation and business rule coordination
- **Mapping Profiles** - AutoMapper profiles for Domain ↔ DTO mapping
- **Transaction Coordination** - Manage consistency across aggregates

#### What Belongs Here

```csharp
// ✅ Use Case - Single business operation
public class PlaceOrderUseCase
{
    private readonly IOrderRepository _orderRepository;
    private readonly ICustomerRepository _customerRepository;
    private readonly IProductRepository _productRepository;
    private readonly IMapper _mapper;

    public PlaceOrderUseCase(
        IOrderRepository orderRepository,
        ICustomerRepository customerRepository,
        IProductRepository productRepository,
        IMapper mapper)
    {
        _orderRepository = orderRepository;
        _customerRepository = customerRepository;
        _productRepository = productRepository;
        _mapper = mapper;
    }

    public async Task<Result<OrderDto>> ExecuteAsync(PlaceOrderCommand command)
    {
        // 1. Load Customer aggregate
        var customer = await _customerRepository.GetByIdAsync(command.CustomerId);
        if (customer == null)
            return Result<OrderDto>.Failure("Customer not found");

        // 2. Create Order aggregate
        var order = new Order(customer.Id, command.ShippingAddress);

        // 3. Coordinate with Product aggregates
        foreach (var itemCommand in command.Items)
        {
            var product = await _productRepository.GetByIdAsync(itemCommand.ProductId);
            if (product == null)
                return Result<OrderDto>.Failure($"Product {itemCommand.ProductId} not found");

            if (!product.IsAvailable(itemCommand.Quantity))
                return Result<OrderDto>.Failure($"Product {product.Name} is not available");

            order.AddItem(product, itemCommand.Quantity);
        }

        // 4. Save Order aggregate
        _orderRepository.Add(order);
        await _orderRepository.SaveChangesAsync();

        // 5. Map domain entity to DTO using AutoMapper
        var orderDto = _mapper.Map<OrderDto>(order);
        return Result<OrderDto>.Success(orderDto);
    }
}

// ✅ Use Case - Confirm Order operation
public class ConfirmOrderUseCase
{
    private readonly IOrderRepository _orderRepository;
    private readonly IMapper _mapper;

    public ConfirmOrderUseCase(IOrderRepository orderRepository, IMapper mapper)
    {
        _orderRepository = orderRepository;
        _mapper = mapper;
    }

    public async Task<Result<OrderDto>> ExecuteAsync(Guid orderId)
    {
        var order = await _orderRepository.GetByIdAsync(orderId);
        if (order == null)
            return Result<OrderDto>.Failure("Order not found");

        // Business logic handled by aggregate
        order.Confirm();

        await _orderRepository.SaveChangesAsync();
        
        // Domain events will be dispatched automatically
        // They will trigger inventory reservation, payment processing, etc.

        var orderDto = _mapper.Map<OrderDto>(order);
        return Result<OrderDto>.Success(orderDto);
    }
}

// ✅ Use Case - Get Order query operation
public class GetOrderUseCase
{
    private readonly IOrderRepository _orderRepository;
    private readonly IMapper _mapper;

    public GetOrderUseCase(IOrderRepository orderRepository, IMapper mapper)
    {
        _orderRepository = orderRepository;
        _mapper = mapper;
    }

    public async Task<Result<OrderDto>> ExecuteAsync(Guid orderId)
    {
        var order = await _orderRepository.GetByIdAsync(orderId);
        if (order == null)
            return Result<OrderDto>.Failure("Order not found");

        var orderDto = _mapper.Map<OrderDto>(order);
        return Result<OrderDto>.Success(orderDto);
    }
}

// ✅ Domain Event Handler - Cross-aggregate coordination
public class OrderConfirmedEventHandler : INotificationHandler<OrderConfirmedEvent>
{
    private readonly IProductRepository _productRepository;
    private readonly ICustomerRepository _customerRepository;
    private readonly IEmailService _emailService;

    public async Task Handle(OrderConfirmedEvent notification, CancellationToken cancellationToken)
    {
        // Reserve inventory in Product aggregates
        var order = await _orderRepository.GetByIdAsync(notification.OrderId);
        foreach (var item in order.Items)
        {
            var product = await _productRepository.GetByIdAsync(item.ProductId);
            product.ReserveStock(item.Quantity);
            await _productRepository.SaveChangesAsync();
        }

        // Award loyalty points to Customer aggregate
        var customer = await _customerRepository.GetByIdAsync(notification.CustomerId);
        customer.AwardLoyaltyPoints((int)notification.TotalAmount.Amount);
        await _customerRepository.SaveChangesAsync();

        // Send confirmation email (external service)
        await _emailService.SendOrderConfirmationAsync(notification.OrderId);
    }
}

// ✅ AutoMapper Profile - Domain to DTO mapping
public class OrderMappingProfile : Profile
{
    public OrderMappingProfile()
    {
        CreateMap<Order, OrderDto>()
            .ForMember(dest => dest.Total, opt => opt.MapFrom(src => src.Total.Amount))
            .ForMember(dest => dest.Currency, opt => opt.MapFrom(src => src.Total.Currency))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()));

        CreateMap<OrderItem, OrderItemDto>()
            .ForMember(dest => dest.UnitPrice, opt => opt.MapFrom(src => src.UnitPrice.Amount))
            .ForMember(dest => dest.LineTotal, opt => opt.MapFrom(src => src.LineTotal.Amount));

        CreateMap<Customer, CustomerDto>()
            .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => $"{src.FirstName} {src.LastName}"));

        CreateMap<Product, ProductDto>()
            .ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.Price.Amount));
    }
}

// ✅ Commands and Queries
public class PlaceOrderCommand
{
    public Guid CustomerId { get; set; }
    public Address ShippingAddress { get; set; }
    public List<OrderItemCommand> Items { get; set; }
}

public class OrderItemCommand
{
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
}

// ✅ DTOs - Data transfer between layers
public class OrderDto
{
    public Guid Id { get; set; }
    public string OrderNumber { get; set; }
    public Guid CustomerId { get; set; }
    public string Status { get; set; }
    public decimal Total { get; set; }
    public string Currency { get; set; }
    public List<OrderItemDto> Items { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class OrderItemDto
{
    public Guid ProductId { get; set; }
    public string ProductName { get; set; }
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
    public decimal LineTotal { get; set; }
}
```

---

### 🌐 API Layer (Presentation)

**Purpose**: Handles external communication using Minimal APIs, directly using application request/response models without additional mapping layers, and coordinates with use cases for specific operations.

#### Responsibilities

- **Minimal API Endpoints** - Handle HTTP requests using feature-based endpoint organization
- **Request/Response Models** - Use application layer models directly (no DTO mapping)
- **Authentication** - User authentication and authorization
- **Input Validation** - Validate incoming requests (handled by application layer)
- **Error Handling** - Convert exceptions to appropriate HTTP responses via middleware
- **API Documentation** - OpenAPI/Swagger documentation

#### What Belongs Here

```csharp
// ✅ Minimal API Endpoints using feature-based organization
public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var auth = endpoints.MapGroup("/api/auth")
            .WithTags("Authentication")
            .WithOpenApi();

        // Login endpoint - uses application models directly
        auth.MapPost("/login", async (
            LoginRequest request,
            IMediator mediator) =>
        {
            // Direct use of application request model (no mapping needed)
            var result = await mediator.Send(request);
            
            return result.IsSuccess 
                ? Results.Ok(result.Value) 
                : Results.BadRequest(new { message = result.Error });
        })
        .WithName("Login")
        .WithSummary("Authenticate user and return JWT token")
        .Produces<LoginResponse>(200)
        .Produces<ErrorResponse>(400);

        // Register endpoint - uses application models directly  
        auth.MapPost("/register", async (
            RegisterRequest request,
            IMediator mediator) =>
        {
            // Direct use of application request model (no mapping needed)
            var result = await mediator.Send(request);
            
            return result.IsSuccess
                ? Results.Created($"/api/users/{result.Value.Id}", result.Value)
                : Results.BadRequest(new { message = result.Error });
        })
        .WithName("Register")
        .WithSummary("Register a new user account")
        .Produces<RegisterResponse>(201)
        .Produces<ErrorResponse>(400);
    }
}

// ✅ Feature-based endpoint organization
public static class OrderEndpoints
{
    public static void MapOrderEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var orders = endpoints.MapGroup("/api/orders")
            .WithTags("Orders")
            .WithOpenApi();

        orders.MapPost("/", async (
            PlaceOrderRequest request,
            IMediator mediator) =>
        {
            var result = await mediator.Send(request);
            return result.IsSuccess
                ? Results.Created($"/api/orders/{result.Value.Id}", result.Value)
                : Results.BadRequest(new { message = result.Error });
        });

        orders.MapGet("/{id:guid}", async (
            Guid id,
            IMediator mediator) =>
        {
            var query = new GetOrderQuery(id);
            var result = await mediator.Send(query);
            return result.IsSuccess ? Results.Ok(result.Value) : Results.NotFound();
        });
    public async Task<ActionResult<OrderResponse>> ConfirmOrder(Guid id)
    {
        var result = await _confirmOrderUseCase.ExecuteAsync(id);

        if (!result.IsSuccess)
            return BadRequest(new ErrorResponse { Message = result.Error });

        var response = _mapper.Map<OrderResponse>(result.Value);
        return Ok(response);
    }
}

// ✅ Request Models
public class PlaceOrderRequest
{
    [Required]
    public Guid CustomerId { get; set; }

    [Required]
    public AddressRequest ShippingAddress { get; set; }

    [Required]
    [MinLength(1)]
    public List<OrderItemRequest> Items { get; set; }
}

public class OrderItemRequest
{
    [Required]
    public Guid ProductId { get; set; }

    [Range(1, int.MaxValue)]
    public int Quantity { get; set; }
}

public class AddressRequest
{
    [Required]
    public string Street { get; set; }

    [Required]
    public string City { get; set; }

    [Required]
    public string State { get; set; }

    [Required]
    public string Country { get; set; }

    [Required]
    public string ZipCode { get; set; }
}

// ✅ Response Models
public class OrderResponse
{
    public Guid Id { get; set; }
    public string OrderNumber { get; set; }
    public Guid CustomerId { get; set; }
    public string Status { get; set; }
    public decimal Total { get; set; }
    public string Currency { get; set; }
    public List<OrderItemResponse> Items { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class OrderItemResponse
{
    public Guid ProductId { get; set; }
    public string ProductName { get; set; }
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
    public decimal LineTotal { get; set; }
}

public class ErrorResponse
{
    public string Message { get; set; }
    public Dictionary<string, string[]> Errors { get; set; }
}
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

// ✅ Utilities
public static class GuidUtilities
{
    public static Guid NewSequentialGuid()
    {
        // Implementation for sequential GUID generation for database performance
        var guidBytes = Guid.NewGuid().ToByteArray();
        var dateTime = DateTime.UtcNow;
        var dateTimeBytes = BitConverter.GetBytes(dateTime.Ticks);

        if (BitConverter.IsLittleEndian)
        {
            Array.Reverse(dateTimeBytes);
        }

        Array.Copy(dateTimeBytes, 2, guidBytes, 10, 6);
        return new Guid(guidBytes);
    }
}

public static class HashingUtilities
{
    public static string HashPassword(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password);
    }

    public static bool VerifyPassword(string password, string hashedPassword)
    {
        return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
    }
}
```

---

## Dependency Rules

### The Fundamental Rule with Shared Layer

**Dependencies can only point inward, with the Shared layer being accessible to all other layers without creating coupling between business layers.**

```
✅ Allowed Dependencies:
API → Application → Domain
Infrastructure → Application → Domain
Infrastructure → Domain
All Layers → Shared

❌ Forbidden Dependencies:
Domain → Application
Domain → Infrastructure  
Domain → API
Application → API
Application → Infrastructure
```

### AutoMapper Dependencies

```
✅ AutoMapper Usage (Simplified - Minimal APIs):
- IMapper injected only into Use Cases (Application Layer)
- Mapping Profiles defined only in Application layer
- Application Layer: Domain ↔ Response mapping (eliminates DTOs)
- API Layer: Uses Request/Response models directly with MediatR (no mapping needed)

❌ AutoMapper Forbidden:
- Direct AutoMapper usage in Domain Layer
- Mapping in API Layer (Minimal APIs use application models directly)
- Multiple mapping profiles for the same transformation
- Complex mapping configurations for simple property copying
```

### Shared Layer Dependencies

```csharp
// ✅ Shared layer can be referenced by all layers
// Program.cs - Dependency Registration (Minimal APIs)
services.AddAutoMapper(
    typeof(AuthMappingProfile)      // Application layer profiles only
);

// Application Layer using Shared
public class PlaceOrderUseCase
{
    public async Task<Result<OrderDto>> ExecuteAsync(PlaceOrderCommand command)
    {
        // Using Result<T> from Shared layer
        if (!command.Items.Any())
            return Result<OrderDto>.Failure("Order must contain items");
    }
}

// API Layer using Shared
[HttpGet]
public async Task<ActionResult<PagedResult<OrderResponse>>> GetOrders([FromQuery] PagedQuery query)
{
    // Using PagedQuery and PagedResult from Shared layer
}

// Domain Layer using Shared (minimal usage)
public class Customer : BaseEntity
{
    public void UpdateEmail(string email)
    {
        // Using extension method from Shared layer
        if (!email.IsValidEmail())
            throw new DomainException("Invalid email address");
    }
}
```

---

## Implementation Guidelines

### Application Layer - Use Case Implementation

#### Single Responsibility Use Cases

```csharp
// ✅ Each use case handles one business operation
public class PlaceOrderUseCase
{
    private readonly IOrderRepository _orderRepository;
    private readonly ICustomerRepository _customerRepository;
    private readonly IProductRepository _productRepository;
    private readonly IMapper _mapper;

    public async Task<Result<OrderDto>> ExecuteAsync(PlaceOrderCommand command)
    {
        // Single focused responsibility: Place an order
        // 1. Validate inputs
        // 2. Load required aggregates
        // 3. Execute business logic
        // 4. Save changes
        // 5. Map and return result
    }
}

public class ConfirmOrderUseCase
{
    // Single responsibility: Confirm an order
}

public class CancelOrderUseCase
{
    // Single responsibility: Cancel an order
}

public class GetOrderUseCase
{
    // Single responsibility: Retrieve an order
}
```

#### AutoMapper Integration in Use Cases

```csharp
public class GetCustomerOrdersUseCase
{
    private readonly IOrderRepository _orderRepository;
    private readonly IMapper _mapper;

    public GetCustomerOrdersUseCase(IOrderRepository orderRepository, IMapper mapper)
    {
        _orderRepository = orderRepository;
        _mapper = mapper;
    }

    public async Task<Result<PagedResult<OrderDto>>> ExecuteAsync(Guid customerId, PagedQuery query)
    {
        // 1. Load orders from domain
        var orders = await _orderRepository.GetByCustomerIdAsync(customerId);

        // 2. Apply filtering and pagination using Shared utilities
        var filteredOrders = orders.Where(o => 
            string.IsNullOrEmpty(query.SearchTerm) || 
            o.OrderNumber.Contains(query.SearchTerm));

        // 3. Convert to paginated result
        var pagedOrders = filteredOrders.ToPagedResult(query.PageNumber, query.PageSize);

        // 4. Map domain entities to DTOs
        var orderDtos = _mapper.Map<IEnumerable<OrderDto>>(pagedOrders.Items);

        var result = new PagedResult<OrderDto>
        {
            Items = orderDtos,
            TotalCount = pagedOrders.TotalCount,
            PageNumber = pagedOrders.PageNumber,
            PageSize = pagedOrders.PageSize
        };

        return Result<PagedResult<OrderDto>>.Success(result);
    }
}
```

### API Layer - Minimal API Implementation with MediatR

#### Endpoints Using Application Models Directly

```csharp
public static class CustomerEndpoints
{
    public static void MapCustomerEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var customers = endpoints.MapGroup("/api/customers")
            .WithTags("Customers")
            .WithOpenApi();

        customers.MapPost("/", async (
            RegisterCustomerRequest request,
            IMediator mediator) =>
        {
            // Use application request model directly (no mapping)
            var result = await mediator.Send(request);

            return result.IsSuccess
                ? Results.Created($"/api/customers/{result.Value.Id}", result.Value)
                : Results.BadRequest(new { message = result.Error });
        })
        .WithName("RegisterCustomer")
        .Produces<RegisterCustomerResponse>(201)
        .Produces<ErrorResponse>(400);

        customers.MapGet("/{id:guid}", async (
            Guid id,
            IMediator mediator) =>
        {
            var query = new GetCustomerQuery(id);
            var result = await mediator.Send(query);

            return result.IsSuccess 
                ? Results.Ok(result.Value) 
                : Results.NotFound();
        })
        .WithName("GetCustomer")
        .Produces<CustomerResponse>(200)
        .Produces(404);
    }
}

// ✅ Program.cs - Endpoint Registration
var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));
builder.Services.AddAutoMapper(typeof(AuthMappingProfile));

var app = builder.Build();

// Map endpoints
app.MapAuthEndpoints();
app.MapCustomerEndpoints();
app.MapOrderEndpoints();

app.Run();
            return NotFound();

        var response = _mapper.Map<CustomerResponse>(result.Value);
        return Ok(response);
    }
}
```

### Shared Layer Usage Examples

#### Cross-Layer Utilities

```csharp
// Used in Domain Layer
public class Email : ValueObject
{
    public string Value { get; }

    public Email(string value)
    {
        if (!value.IsValidEmail()) // Extension from Shared layer
            throw new DomainException("Invalid email address");

        Value = value;
    }
}

// Used in Application Layer  
public class SearchProductsUseCase
{
    public async Task<Result<PagedResult<ProductDto>>> ExecuteAsync(SearchProductsQuery query)
    {
        // Using PagedQuery and PagedResult from Shared layer
        var products = await _productRepository.SearchAsync(query.SearchTerm);
        var pagedResult = products.ToPagedResult(query.PageNumber, query.PageSize);
        
        var productDtos = _mapper.Map<IEnumerable<ProductDto>>(pagedResult.Items);
        
        return Result<PagedResult<ProductDto>>.Success(new PagedResult<ProductDto>
        {
            Items = productDtos,
            TotalCount = pagedResult.TotalCount,
            PageNumber = pagedResult.PageNumber,
            PageSize = pagedResult.PageSize
        });
    }
}

// Used in API Layer
[HttpGet("search")]
public async Task<ActionResult<PagedResult<ProductResponse>>> SearchProducts([FromQuery] PagedQuery query)
{
    // Validate page size using Shared constants
    if (query.PageSize > ApplicationConstants.MAX_PAGE_SIZE)
        query.PageSize = ApplicationConstants.MAX_PAGE_SIZE;

    var searchQuery = _mapper.Map<SearchProductsQuery>(query);
    var result = await _searchProductsUseCase.ExecuteAsync(searchQuery);

    if (!result.IsSuccess)
        return BadRequest(new ErrorResponse { Message = result.Error });

    var response = new PagedResult<ProductResponse>
    {
        Items = _mapper.Map<IEnumerable<ProductResponse>>(result.Value.Items),
        TotalCount = result.Value.TotalCount,
        PageNumber = result.Value.PageNumber,
        PageSize = result.Value.PageSize
    };

    return Ok(response);
}
```

---

## Folder Structure

### Complete Project Structure with Use Cases and Shared Layer

```
CompanyName.ProjectName/
├── src/
│   ├── CompanyName.ProjectName.Shared/               # 🔧 Shared Layer
│   │   ├── Constants/
│   │   │   ├── ApplicationConstants.cs
│   │   │   └── ConfigurationKeys.cs
│   │   │
│   │   ├── Extensions/
│   │   │   ├── StringExtensions.cs
│   │   │   ├── EnumerableExtensions.cs
│   │   │   └── DateTimeExtensions.cs
│   │   │
│   │   ├── Models/
│   │   │   ├── Result.cs
│   │   │   ├── PagedResult.cs
│   │   │   └── PagedQuery.cs
│   │   │
│   │   ├── Utilities/
│   │   │   ├── GuidUtilities.cs
│   │   │   ├── HashingUtilities.cs
│   │   │   └── ValidationHelpers.cs
│   │   │
│   │   ├── Enums/
│   │   │   ├── SortDirection.cs
│   │   │   └── OperationResult.cs
│   │   │
│   │   └── CompanyName.ProjectName.Shared.csproj
│   │
│   ├── CompanyName.ProjectName.Domain/              # 🎯 Domain Layer
│   │   ├── Common/
│   │   │   ├── BaseEntity.cs
│   │   │   ├── AggregateRoot.cs
│   │   │   ├── IAggregateRoot.cs
│   │   │   ├── ValueObject.cs
│   │   │   ├── DomainEvent.cs
│   │   │   └── DomainException.cs
│   │   │
│   │   ├── Aggregates/
│   │   │   ├── OrderAggregate/
│   │   │   │   ├── Order.cs                         # Aggregate Root
│   │   │   │   ├── OrderItem.cs                     # Entity
│   │   │   │   ├── OrderStatus.cs                   # Enum
│   │   │   │   ├── Events/
│   │   │   │   │   ├── OrderCreatedEvent.cs
│   │   │   │   │   ├── OrderConfirmedEvent.cs
│   │   │   │   │   ├── OrderItemAddedEvent.cs
│   │   │   │   │   └── OrderCancelledEvent.cs
│   │   │   │   └── IOrderRepository.cs
│   │   │   │
│   │   │   ├── CustomerAggregate/
│   │   │   │   ├── Customer.cs                      # Aggregate Root
│   │   │   │   ├── CustomerTier.cs                  # Enum
│   │   │   │   ├── CustomerStatus.cs                # Enum
│   │   │   │   ├── Events/
│   │   │   │   │   ├── CustomerRegisteredEvent.cs
│   │   │   │   │   ├── CustomerTierChangedEvent.cs
│   │   │   │   │   └── LoyaltyPointsAwardedEvent.cs
│   │   │   │   └── ICustomerRepository.cs
│   │   │   │
│   │   │   ├── ProductAggregate/
│   │   │   │   ├── Product.cs                       # Aggregate Root
│   │   │   │   ├── ProductStatus.cs                 # Enum
│   │   │   │   ├── ProductCategory.cs               # Enum
│   │   │   │   ├── Events/
│   │   │   │   │   ├── ProductCreatedEvent.cs
│   │   │   │   │   ├── PriceChangedEvent.cs
│   │   │   │   │   ├── StockReservedEvent.cs
│   │   │   │   │   └── LowStockEvent.cs
│   │   │   │   └── IProductRepository.cs
│   │   │   │
│   │   │   └── PaymentAggregate/
│   │   │       ├── Payment.cs                       # Aggregate Root
│   │   │       ├── PaymentStatus.cs                 # Enum
│   │   │       ├── PaymentMethod.cs                 # Value Object
│   │   │       ├── Events/
│   │   │       │   ├── PaymentInitiatedEvent.cs
│   │   │       │   ├── PaymentSucceededEvent.cs
│   │   │       │   └── PaymentFailedEvent.cs
│   │   │       └── IPaymentRepository.cs
│   │   │
│   │   ├── ValueObjects/
│   │   │   ├── Money.cs
│   │   │   ├── Address.cs
│   │   │   ├── Email.cs
│   │   │   └── PhoneNumber.cs
│   │   │
│   │   └── Services/
│   │       ├── IDomainService.cs
│   │       └── PricingService.cs
│   │
│   ├── CompanyName.ProjectName.Application/         # 🔧 Application Layer
│   │   ├── Common/
│   │   │   ├── Interfaces/
│   │   │   │   ├── ICurrentUserService.cs
│   │   │   │   ├── IEmailService.cs
│   │   │   │   └── ICacheService.cs
│   │   │   │
│   │   │   └── Behaviors/
│   │   │       ├── ValidationBehavior.cs
│   │   │       └── LoggingBehavior.cs
│   │   │
│   │   ├── UseCases/                                # 🎯 Use Cases (Single Operations)
│   │   │   ├── Orders/
│   │   │   │   ├── PlaceOrderUseCase.cs
│   │   │   │   ├── ConfirmOrderUseCase.cs
│   │   │   │   ├── CancelOrderUseCase.cs
│   │   │   │   ├── GetOrderUseCase.cs
│   │   │   │   └── GetCustomerOrdersUseCase.cs
│   │   │   │
│   │   │   ├── Customers/
│   │   │   │   ├── RegisterCustomerUseCase.cs
│   │   │   │   ├── UpdateCustomerUseCase.cs
│   │   │   │   ├── GetCustomerUseCase.cs
│   │   │   │   └── DeactivateCustomerUseCase.cs
│   │   │   │
│   │   │   ├── Products/
│   │   │   │   ├── CreateProductUseCase.cs
│   │   │   │   ├── UpdateProductPriceUseCase.cs
│   │   │   │   ├── GetProductUseCase.cs
│   │   │   │   ├── SearchProductsUseCase.cs
│   │   │   │   └── RestockProductUseCase.cs
│   │   │   │
│   │   │   └── Payments/
│   │   │       ├── ProcessPaymentUseCase.cs
│   │   │       ├── RefundPaymentUseCase.cs
│   │   │       └── GetPaymentUseCase.cs
│   │   │
│   │   ├── EventHandlers/                           # Cross-aggregate coordination
│   │   │   ├── OrderEventHandlers/
│   │   │   │   ├── OrderConfirmedEventHandler.cs
│   │   │   │   └── OrderCancelledEventHandler.cs
│   │   │   │
│   │   │   ├── CustomerEventHandlers/
│   │   │   │   └── CustomerRegisteredEventHandler.cs
│   │   │   │
│   │   │   └── ProductEventHandlers/
│   │   │       └── LowStockEventHandler.cs
│   │   │
│   │   ├── Commands/
│   │   │   ├── PlaceOrderCommand.cs
│   │   │   ├── ConfirmOrderCommand.cs
│   │   │   ├── RegisterCustomerCommand.cs
│   │   │   └── CreateProductCommand.cs
│   │   │
│   │   ├── Queries/
│   │   │   ├── GetOrderQuery.cs
│   │   │   ├── GetCustomerOrdersQuery.cs
│   │   │   ├── SearchProductsQuery.cs
│   │   │   └── GetCustomerQuery.cs
│   │   │
│   │   ├── DTOs/                                    # Data Transfer Objects
│   │   │   ├── OrderDto.cs
│   │   │   ├── CustomerDto.cs
│   │   │   ├── ProductDto.cs
│   │   │   └── PaymentDto.cs
│   │   │
│   │   ├── Mappings/                                # AutoMapper Profiles
│   │   │   ├── OrderMappingProfile.cs               # Domain → DTO
│   │   │   ├── CustomerMappingProfile.cs            # Domain → DTO
│   │   │   ├── ProductMappingProfile.cs             # Domain → DTO
│   │   │   └── PaymentMappingProfile.cs             # Domain → DTO
│   │   │
│   │   ├── Validators/
│   │   │   ├── PlaceOrderCommandValidator.cs
│   │   │   ├── RegisterCustomerCommandValidator.cs
│   │   │   └── CreateProductCommandValidator.cs
│   │   │
│   │   └── CompanyName.ProjectName.Application.csproj
│   │
│   ├── CompanyName.ProjectName.Infrastructure/      # 🏗️ Infrastructure Layer
│   │   ├── Data/
│   │   │   ├── ApplicationDbContext.cs
│   │   │   │
│   │   │   ├── Configurations/                      # Aggregate Configurations
│   │   │   │   ├── OrderConfiguration.cs
│   │   │   │   ├── CustomerConfiguration.cs
│   │   │   │   ├── ProductConfiguration.cs
│   │   │   │   └── PaymentConfiguration.cs
│   │   │   │
│   │   │   └── Migrations/
│   │   │
│   │   ├── Repositories/                            # Aggregate Repositories
│   │   │   ├── OrderRepository.cs
│   │   │   ├── CustomerRepository.cs
│   │   │   ├── ProductRepository.cs
│   │   │   └── PaymentRepository.cs
│   │   │
│   │   ├── Services/
│   │   │   ├── EmailService.cs
│   │   │   ├── CurrentUserService.cs
│   │   │   └── CacheService.cs
│   │   │
│   │   └── Extensions/
│   │       └── ServiceCollectionExtensions.cs
│   │
│   └── CompanyName.ProjectName.API/                 # 🌐 API Layer
│       ├── Endpoints/
│       │   ├── AuthEndpoints.cs                     # Authentication endpoints (Login, Register)
│       │   ├── OrderEndpoints.cs                    # Order management endpoints
│       │   ├── CustomerEndpoints.cs                 # Customer management endpoints
│       │   └── ProductEndpoints.cs                  # Product catalog endpoints
│       │
│       ├── Models/
│       │   └── ErrorResponse.cs                     # API error response model
│       │
│       ├── Mappings/                                # AutoMapper Profiles
│       │   ├── ApiMappingProfile.cs                 # Request/Response ↔ DTO
│       │   └── RequestMappingProfile.cs             # Additional API mappings
│       │
│       ├── Middleware/
│       │   ├── ExceptionHandlingMiddleware.cs
│       │   └── RequestLoggingMiddleware.cs
│       │
│       ├── Extensions/
│       │   ├── ServiceCollectionExtensions.cs
│       │   └── ApplicationBuilderExtensions.cs
│       │
│       ├── Program.cs
│       ├── appsettings.json
│       └── CompanyName.ProjectName.API.csproj
│
└── tests/                                           # 🧪 Test Projects
    ├── CompanyName.ProjectName.Shared.UnitTests/
    ├── CompanyName.ProjectName.Domain.UnitTests/
    ├── CompanyName.ProjectName.Application.UnitTests/
    ├── CompanyName.ProjectName.Infrastructure.IntegrationTests/
    └── CompanyName.ProjectName.API.IntegrationTests/
```

---

## Design Patterns

### Use Case Pattern

```csharp
// ✅ Single Responsibility Use Case
public class PlaceOrderUseCase
{
    private readonly IOrderRepository _orderRepository;
    private readonly ICustomerRepository _customerRepository;
    private readonly IProductRepository _productRepository;
    private readonly IMapper _mapper;

    public PlaceOrderUseCase(
        IOrderRepository orderRepository,
        ICustomerRepository customerRepository, 
        IProductRepository productRepository,
        IMapper mapper)
    {
        _orderRepository = orderRepository;
        _customerRepository = customerRepository;
        _productRepository = productRepository;
        _mapper = mapper;
    }

    public async Task<Result<OrderDto>> ExecuteAsync(PlaceOrderCommand command)
    {
        // 1. Single, focused responsibility
        // 2. Maps directly to business use case
        // 3. Easy to test in isolation
        // 4. Clear dependencies
        // 5. Uses AutoMapper for domain → DTO transformation
    }
}

// ✅ Controller delegates to specific use cases
[ApiController]
public class OrdersController : ControllerBase
{
    private readonly PlaceOrderUseCase _placeOrderUseCase;
    private readonly GetOrderUseCase _getOrderUseCase;
    private readonly ConfirmOrderUseCase _confirmOrderUseCase;

    [HttpPost]
    public async Task<ActionResult<OrderResponse>> PlaceOrder(PlaceOrderRequest request)
    {
        var command = _mapper.Map<PlaceOrderCommand>(request);
        var result = await _placeOrderUseCase.ExecuteAsync(command);
        // Handle result and map to response
    }
}
```

### AutoMapper Pattern Integration

```csharp
// ✅ Application Layer Mapping Profile - Domain to DTO
public class OrderMappingProfile : Profile
{
    public OrderMappingProfile()
    {
        CreateMap<Order, OrderDto>()
            .ForMember(dest => dest.Total, opt => opt.MapFrom(src => src.Total.Amount))
            .ForMember(dest => dest.Currency, opt => opt.MapFrom(src => src.Total.Currency));

        CreateMap<OrderItem, OrderItemDto>()
            .ForMember(dest => dest.UnitPrice, opt => opt.MapFrom(src => src.UnitPrice.Amount));
    }
}

// ✅ No API Layer Mapping Profile Needed - Minimal APIs use application models directly

// ✅ Dependency Registration (Minimal APIs)
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddMappingProfiles(this IServiceCollection services)
    {
        services.AddAutoMapper(
            typeof(OrderMappingProfile)     // Application layer only
        );
        
        return services;
    }
}
```

### Repository Pattern (Per Aggregate)

```csharp
// ✅ One repository interface per aggregate root
public interface IOrderRepository
{
    Task<Order> GetByIdAsync(Guid id);
    Task<IEnumerable<Order>> GetByCustomerIdAsync(Guid customerId);
    void Add(Order order);
    void Update(Order order);
    Task SaveChangesAsync();
}

// ✅ Repository loads complete aggregates
public class OrderRepository : IOrderRepository
{
    public async Task<Order> GetByIdAsync(Guid id)
    {
        return await _context.Orders
            .Include(o => o.Items)  // Load complete aggregate
            .FirstOrDefaultAsync(o => o.Id == id);
    }
}
```

### Shared Layer Utilities Pattern

```csharp
// ✅ Common Result Pattern - Used across all layers
public class Result<T>
{
    public bool IsSuccess { get; }
    public T Value { get; }
    public string Error { get; }
    
    // Used in Use Cases
    public static Result<T> Success(T value) => new(true, value, string.Empty);
    public static Result<T> Failure(string error) => new(false, default, error);
}

// ✅ Extension Methods - Available to all layers
public static class EnumerableExtensions
{
    public static PagedResult<T> ToPagedResult<T>(this IEnumerable<T> source, int pageNumber, int pageSize)
    {
        // Implementation used by Use Cases and Controllers
    }
}
```
