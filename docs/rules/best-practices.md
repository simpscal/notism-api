# Notism API - Best Practices

## 📋 Table of Contents

1. [Repository Pattern](#repository-pattern)
2. [Specification Pattern](#specification-pattern)
3. [Domain Events](#domain-events)
4. [Handler Design](#handler-design)
5. [Validation](#validation)
6. [Error Handling](#error-handling)
7. [Naming Conventions](#naming-conventions)
8. [Code Organization](#code-organization)

---

## Repository Pattern

### Use Specifications with IRepository<T> Methods

**✅ DO: Use Specifications with IRepository<T> Methods**

```csharp
// In Handler
var specification = new CartItemByUserIdSpecification(userId);
var cartItems = await _cartItemRepository.FilterByExpressionAsync(specification);

// Repository interface stays clean
public interface ICartItemRepository : IRepository<CartItem>
{
    // No custom query methods needed for simple queries
}
```

**❌ DON'T: Add Custom Query Methods to Repository Interfaces**

```csharp
// Avoid this pattern for simple queries
public interface ICartItemRepository : IRepository<CartItem>
{
    Task<IEnumerable<CartItem>> GetCartItemsByUserIdAsync(Guid userId);
    Task<CartItem?> FindCartItemByUserAndFoodAsync(Guid userId, Guid foodId);
}
```

**When to Add Custom Repository Methods**

Only add custom methods to repository interfaces when:
- The operation involves complex business logic that belongs in the repository
- The operation is a specific domain operation (e.g., `ClearCartByUserIdAsync`)
- The operation cannot be easily expressed as a specification

```csharp
// ✅ Acceptable: Complex operation that encapsulates business logic
public interface ICartItemRepository : IRepository<CartItem>
{
    Task ClearCartByUserIdAsync(Guid userId); // Uses specifications internally
}
```

### Why Use Specifications?

1. **Separation of Concerns**: Query logic lives in Domain layer as specifications, not Infrastructure
2. **Reusability**: Specifications can be composed and reused across different queries
3. **Testability**: Specifications are easy to unit test independently
4. **Consistency**: All repositories follow the same pattern using `IRepository<T>`
5. **Flexibility**: Easy to combine specifications using `And()`, `Or()`, `Not()` methods
6. **Maintainability**: Query logic is centralized in specification classes, not scattered in repository methods

---

## Specification Pattern

### Creating Specifications

**✅ DO: Create Specifications in Domain Layer**

```csharp
// Domain/Cart/Specifications/CartItemByUserIdSpecification.cs
public class CartItemByUserIdSpecification : Specification<CartItem>
{
    private readonly Guid _userId;

    public CartItemByUserIdSpecification(Guid userId)
    {
        _userId = userId;
        Include(c => c.Food);
        Include(c => c.Food.Images);
    }

    public override Expression<Func<CartItem, bool>> ToExpression()
    {
        return cartItem => cartItem.UserId == _userId;
    }
}
```

**✅ DO: Use Specifications in Handlers**

```csharp
// Application/Cart/GetCartItems/GetCartItemsHandler.cs
public async Task<GetCartItemsResponse> Handle(
    GetCartItemsRequest request,
    CancellationToken cancellationToken)
{
    var specification = new CartItemByUserIdSpecification(request.UserId);
    var cartItems = await _cartItemRepository.FilterByExpressionAsync(specification);
    
    // Map to response...
}
```

**✅ DO: Include Related Entities in Specifications**

```csharp
public class CartItemByUserIdSpecification : Specification<CartItem>
{
    public CartItemByUserIdSpecification(Guid userId)
    {
        _userId = userId;
        Include(c => c.Food);           // Include navigation property
        Include(c => c.Food.Images);    // Include nested navigation
    }
}
```

**❌ DON'T: Put Query Logic in Handlers**

```csharp
// Avoid this pattern
var cartItems = await _cartItemRepository.FilterByExpressionAsync(
    new Specification<CartItem>(c => c.UserId == request.UserId));
```

**✅ DO: Compose Specifications When Needed**

```csharp
var activeItemsSpec = new CartItemByUserIdSpecification(userId);
var availableItemsSpec = new CartItemAvailableSpecification();
var combinedSpec = activeItemsSpec.And(availableItemsSpec);
var items = await _cartItemRepository.FilterByExpressionAsync(combinedSpec);
```

---

## Domain Events

### When to Raise Domain Events

**✅ DO: Raise Events for Important Business State Changes**

```csharp
public class CartItem : AggregateRoot
{
    private CartItem(Guid userId, Guid foodId, int quantity)
    {
        UserId = userId;
        FoodId = foodId;
        Quantity = quantity;
        
        AddDomainEvent(new CartItemAddedEvent(Id, UserId, FoodId, Quantity));
    }

    public void UpdateQuantity(int quantity)
    {
        var oldQuantity = Quantity;
        Quantity = quantity;
        UpdatedAt = DateTime.UtcNow;

        ClearDomainEvents();
        AddDomainEvent(new CartItemQuantityUpdatedEvent(Id, UserId, FoodId, oldQuantity, quantity));
    }
}
```

**✅ DO: Inherit from DomainEvent Base Class**

```csharp
public class CartItemAddedEvent : DomainEvent
{
    public Guid CartItemId { get; }
    public Guid UserId { get; }
    public Guid FoodId { get; }
    public int Quantity { get; }

    public CartItemAddedEvent(Guid cartItemId, Guid userId, Guid foodId, int quantity)
    {
        CartItemId = cartItemId;
        UserId = userId;
        FoodId = foodId;
        Quantity = quantity;
    }
}
```

**❌ DON'T: Raise Events for Every Property Change**

Only raise events for:
- Significant business events (created, updated, deleted)
- Events that other aggregates or bounded contexts need to know about
- Events that trigger side effects (notifications, logging, etc.)

---

## Handler Design

### Keep Handlers Thin

**✅ DO: Orchestrate Domain Operations**

```csharp
public class AddCartItemHandler : IRequestHandler<AddCartItemRequest, AddCartItemResponse>
{
    public async Task<AddCartItemResponse> Handle(
        AddCartItemRequest request,
        CancellationToken cancellationToken)
    {
        // 1. Load aggregates
        var food = await _foodRepository.FindByExpressionAsync(
            new FoodByIdSpecification(request.FoodId))
        ?? throw new ResultFailureException("Food not found");

        // 2. Validate business rules
        if (!food.IsAvailable)
            throw new ResultFailureException("Food is not available");

        // 3. Execute domain operations
        var cartItem = CartItem.Create(request.UserId, request.FoodId, request.Quantity);
        await _cartItemRepository.AddAsync(cartItem);
        await _cartItemRepository.SaveChangesAsync();

        // 4. Return response
        return new AddCartItemResponse { Id = cartItem.Id };
    }
}
```

**❌ DON'T: Put Business Logic in Handlers**

```csharp
// Avoid this pattern
public async Task<AddCartItemResponse> Handle(...)
{
    // Business logic should be in domain, not handler
    if (quantity <= 0)
        throw new Exception("Invalid quantity");
    
    var price = food.Price * quantity; // Business calculation in handler
    // ...
}
```

### Handler Responsibilities

1. **Load Aggregates**: Use repositories to load required aggregates
2. **Validate Business Rules**: Check preconditions before operations
3. **Execute Domain Methods**: Call methods on aggregates (business logic lives here)
4. **Persist Changes**: Save aggregates through repositories
5. **Map to Response**: Transform domain entities to response DTOs

---

## Validation

### Request Validation

**✅ DO: Use FluentValidation for Request Validation**

```csharp
public class AddCartItemRequestValidator : AbstractValidator<AddCartItemRequest>
{
    public AddCartItemRequestValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("UserId is required");

        RuleFor(x => x.FoodId)
            .NotEmpty()
            .WithMessage("FoodId is required");

        RuleFor(x => x.Quantity)
            .GreaterThan(0)
            .WithMessage("Quantity must be greater than zero");
    }
}
```

**✅ DO: Validate in Domain Layer for Business Rules**

```csharp
public static CartItem Create(Guid userId, Guid foodId, int quantity)
{
    if (quantity <= 0)
    {
        throw new ArgumentException("Quantity must be greater than zero", nameof(quantity));
    }

    return new CartItem(userId, foodId, quantity);
}
```

**Validation Layers**

- **Request Validation (FluentValidation)**: Input format, required fields, data types
- **Domain Validation**: Business rules, invariants, constraints
- **Handler Validation**: Cross-aggregate validation, existence checks

---

## Error Handling

### Use ResultFailureException

**✅ DO: Throw ResultFailureException for Business Violations**

```csharp
var food = await _foodRepository.FindByExpressionAsync(
    new FoodByIdSpecification(request.FoodId))
?? throw new ResultFailureException("Food not found");

if (!food.IsAvailable)
{
    throw new ResultFailureException("Food is not available");
}
```

**✅ DO: Use Descriptive Error Messages**

```csharp
// Good: Clear, actionable message
throw new ResultFailureException("Insufficient stock. Available: 5, Requested: 10");

// Bad: Vague message
throw new ResultFailureException("Error");
```

**❌ DON'T: Return Null or Error Codes**

```csharp
// Avoid this pattern
var food = await _foodRepository.FindByExpressionAsync(...);
if (food == null)
{
    return new AddCartItemResponse { Success = false, ErrorCode = 404 };
}
```

---

## Naming Conventions

### Aggregate Roots and Entities

**✅ DO: Use Singular Nouns**

```csharp
public class CartItem : AggregateRoot { }
public class Food : AggregateRoot { }
public class User : AggregateRoot { }
```

### Specifications

**✅ DO: Use Descriptive Names Ending with "Specification"**

```csharp
CartItemByUserIdSpecification
FoodByIdSpecification
UserByEmailSpecification
CartItemByUserAndFoodSpecification
```

### Handlers

**✅ DO: Use Feature Name + "Handler"**

```csharp
GetCartItemsHandler
AddCartItemHandler
UpdateCartItemQuantityHandler
```

### Requests and Responses

**✅ DO: Use Feature Name + "Request"/"Response"**

```csharp
GetCartItemsRequest
GetCartItemsResponse
AddCartItemRequest
AddCartItemResponse
```

### Domain Events

**✅ DO: Use Past Tense for Events**

```csharp
CartItemAddedEvent
CartItemQuantityUpdatedEvent
FoodCreatedEvent
UserProfileUpdatedEvent
```

---

## Code Organization

### Feature-Based Folders

**✅ DO: Organize by Feature**

```
Cart/
├── GetCartItems/
│   ├── GetCartItemsHandler.cs
│   ├── GetCartItemsRequest.cs
│   ├── GetCartItemsResponse.cs
│   └── GetCartItemsRequestValidator.cs
├── AddCartItem/
│   ├── AddCartItemHandler.cs
│   ├── AddCartItemRequest.cs
│   ├── AddCartItemResponse.cs
│   └── AddCartItemRequestValidator.cs
```

**❌ DON'T: Organize by Type**

```
Handlers/
├── GetCartItemsHandler.cs
├── AddCartItemHandler.cs
Requests/
├── GetCartItemsRequest.cs
├── AddCartItemRequest.cs
```

### Domain Layer Organization

**✅ DO: Group by Aggregate**

```
Domain/
├── Cart/
│   ├── CartItem.cs
│   ├── ICartItemRepository.cs
│   ├── Events/
│   │   ├── CartItemAddedEvent.cs
│   │   └── CartItemQuantityUpdatedEvent.cs
│   └── Specifications/
│       ├── CartItemByUserIdSpecification.cs
│       └── CartItemByUserAndFoodSpecification.cs
├── Food/
│   ├── Food.cs
│   ├── FoodImage.cs
│   └── ...
```

### Endpoint Organization

**✅ DO: Group Endpoints by Feature**

```csharp
public static class CartEndpoints
{
    public static void MapCartEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/cart")
            .WithTags("Cart Management")
            .WithOpenApi()
            .RequireAuthorization();

        group.MapGet("/", GetCartItemsAsync);
        group.MapPost("/items", AddCartItemAsync);
        group.MapPatch("/items/{id:guid}", UpdateCartItemQuantityAsync);
        group.MapDelete("/items/{id:guid}", RemoveCartItemAsync);
        group.MapDelete("/", ClearCartAsync);
    }
}
```

---

## Additional Best Practices

### Aggregate Roots

**✅ DO: Make Entities Aggregate Roots When Managed Independently**

```csharp
// If CartItem is managed independently with its own repository
public class CartItem : AggregateRoot { }

// If CartItem is part of a Cart aggregate
public class Cart : AggregateRoot
{
    private readonly List<CartItem> _items = new();
    public IReadOnlyCollection<CartItem> Items => _items.AsReadOnly();
}
```

### Value Objects

**✅ DO: Use Value Objects for Domain Concepts**

```csharp
public class Email : ValueObject
{
    public string Value { get; }

    private Email(string value)
    {
        if (string.IsNullOrWhiteSpace(value) || !IsValidEmail(value))
            throw new ArgumentException("Invalid email address", nameof(value));
        Value = value;
    }

    public static Email Create(string value) => new(value);
}
```

### Private Constructors

**✅ DO: Use Private Constructors with Factory Methods**

```csharp
public class CartItem : AggregateRoot
{
    private CartItem(Guid userId, Guid foodId, int quantity)
    {
        // Initialize...
    }

    public static CartItem Create(Guid userId, Guid foodId, int quantity)
    {
        // Validation
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be greater than zero");

        return new CartItem(userId, foodId, quantity);
    }
}
```

### Include Related Entities

**✅ DO: Always Include Required Navigation Properties**

```csharp
public class CartItemByUserIdSpecification : Specification<CartItem>
{
    public CartItemByUserIdSpecification(Guid userId)
    {
        _userId = userId;
        Include(c => c.Food);           // Required for accessing Food properties
        Include(c => c.Food.Images);    // Required for accessing Images
    }
}
```

---

## Summary

Following these best practices ensures:

- **Maintainability**: Code is organized and easy to understand
- **Testability**: Components can be tested independently
- **Consistency**: Patterns are applied uniformly across the codebase
- **Scalability**: Architecture supports growth and change
- **Quality**: Business rules are properly enforced and validated

