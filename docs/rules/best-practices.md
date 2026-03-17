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

1. **Separation of Concerns**: Query logic lives in Application layer as specifications, not Infrastructure
2. **Reusability**: Specifications can be composed and reused across different queries
3. **Testability**: Specifications are easy to unit test independently
4. **Consistency**: All repositories follow the same pattern using `IRepository<T>`
5. **Flexibility**: Easy to combine specifications using `And()`, `Or()`, `Not()` methods
6. **Maintainability**: Query logic is centralized in specification classes, not scattered in repository methods

---

## Specification Pattern

### When to Use FilterSpecification<T> vs Specific Specification Classes

**✅ DO: Use `FilterSpecification<T>` for Simple Filters**

Use the generic `FilterSpecification<T>` for simple, ad-hoc filtering that doesn't require custom logic or ordering:

```csharp
// Application/Order/GetOrderById/GetOrderByIdHandler.cs
public async Task<GetOrderByIdResponse> Handle(...)
{
    var specification = new FilterSpecification<Order>(
        o => o.Id == request.OrderId && o.UserId == request.UserId)
        .Include("Items.Food.Images")
        .Include(o => o.StatusHistory);
    
    var order = await _orderRepository.FindByExpressionAsync(specification);
    // ...
}
```

**✅ DO: Create Specific Specification Classes for Complex Logic**

Create dedicated specification classes when you need:
- Complex filtering logic with multiple conditions
- Custom ordering (`ApplyOrdering` override)
- Value object handling
- Reusable query logic across multiple handlers

```csharp
// Application/Food/GetFoods/GetFoodsSpecification.cs
public class GetFoodsSpecification : Specification<Food>
{
    private readonly FoodCategory? _category;
    private readonly string? _keyword;
    private readonly bool? _isAvailable;

    public GetFoodsSpecification(
        FoodCategory? category = null,
        string? keyword = null,
        bool? isAvailable = null)
    {
        _category = category;
        _keyword = keyword;
        _isAvailable = isAvailable;
    }

    public override Expression<Func<Food, bool>> ToExpression()
    {
        return food =>
            !food.IsDeleted &&
            (!_category.HasValue || food.Category == _category.Value) &&
            (!_isAvailable.HasValue || food.IsAvailable == _isAvailable.Value) &&
            (string.IsNullOrWhiteSpace(_keyword) ||
                food.Name.ToLower().Contains(_keyword) ||
                food.Description.ToLower().Contains(_keyword));
    }

    public override IQueryable<Food> ApplyOrdering(IQueryable<Food> queryable)
    {
        return queryable.OrderByDescending(f => f.CreatedAt);
    }
}
```

**❌ DON'T: Create Specifications for Simple Filters**

Avoid creating specification classes for simple, one-off filters:

```csharp
// ❌ Don't create a class for this
public class OrderByIdAndUserIdSpecification : Specification<Order>
{
    // Simple filter - use FilterSpecification instead
}

// ✅ Use FilterSpecification instead
var spec = new FilterSpecification<Order>(
    o => o.Id == orderId && o.UserId == userId);
```

### Including Related Entities

**✅ DO: Use Expression-Based Includes for Direct Navigation Properties**

```csharp
var specification = new FilterSpecification<Order>(o => o.UserId == userId)
    .Include(o => o.StatusHistory);  // Direct navigation property
```

**✅ DO: Use String-Based Includes for Nested Navigation Properties**

For nested navigation properties (especially through collections), use string-based includes with dot notation:

```csharp
// Application/Order/GetOrders/GetOrdersHandler.cs
var specification = new FilterSpecification<Order>(o => o.UserId == request.UserId)
    .Include("Items.Food.Images")  // String-based for nested collection navigation
    .Include(o => o.StatusHistory); // Expression-based for direct navigation
```

**Why String-Based Includes?**

EF Core doesn't support `Select` inside `Include` operations. For nested navigation through collections (e.g., `Order.Items.Food.Images`), use string-based includes:

```csharp
// ❌ This will throw InvalidOperationException
.Include(o => o.Items.Select(i => i.Food))

// ✅ Use string-based include instead
.Include("Items.Food.Images")
```

**✅ DO: Combine Expression and String Includes**

You can mix both approaches in the same specification:

```csharp
var specification = new FilterSpecification<Order>(o => o.UserId == userId)
    .Include(o => o.StatusHistory)        // Expression-based
    .Include("Items.Food.Images");        // String-based for nested paths
```

### Composing Specifications

**✅ DO: Compose Specifications for Complex Queries**

Use `And()`, `Or()`, and `Not()` to combine specifications:

```csharp
var activeItemsSpec = new CartItemByUserIdSpecification(userId);
var availableItemsSpec = new CartItemAvailableSpecification();
var combinedSpec = activeItemsSpec.And(availableItemsSpec);
var items = await _cartItemRepository.FilterByExpressionAsync(combinedSpec);
```

**✅ DO: Compose FilterSpecification with Other Specifications**

```csharp
var userOrdersSpec = new FilterSpecification<Order>(o => o.UserId == userId);
var activeOrdersSpec = new FilterSpecification<Order>(o => o.DeliveryStatus != DeliveryStatus.Delivered);
var combinedSpec = userOrdersSpec.And(activeOrdersSpec)
    .Include("Items.Food.Images");
```

### Specification Best Practices

**✅ DO: Create Specifications in Application Layer**

Place specifications in the same feature folder as the handler that uses them:

```csharp
// Application/Food/GetFoods/GetFoodsSpecification.cs
namespace Notism.Application.Food.GetFoods;

public class GetFoodsSpecification : Specification<Food>
{
    // Implementation...
}
```

**✅ DO: Use Descriptive Names Ending with "Specification"**

```csharp
FoodsFilterSpecification
CartItemByUserIdSpecification
UserByEmailSpecification
OrderByIdAndUserIdSpecification
```

**✅ DO: Include Required Navigation Properties**

Always include navigation properties needed for the query:

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

**❌ DON'T: Put Query Logic in Handlers**

```csharp
// ❌ Avoid this pattern
var cartItems = await _cartItemRepository.FilterByExpressionAsync(
    new Specification<CartItem>(c => c.UserId == request.UserId));

// ✅ Use FilterSpecification or create a dedicated specification
var specification = new FilterSpecification<CartItem>(c => c.UserId == request.UserId)
    .Include(c => c.Food);
```

**✅ DO: Use Specifications with Repository Methods**

```csharp
// In Handler
var specification = new FilterSpecification<Order>(o => o.UserId == userId)
    .Include("Items.Food.Images");
var orders = await _orderRepository.FilterByExpressionAsync(specification);
```

### When to Create Specific Specification Classes

Create dedicated specification classes when:

1. **Complex Filtering Logic**: Multiple conditions with business rules
   ```csharp
   // Application/Food/GetFoods/GetFoodsSpecification.cs
   public class GetFoodsSpecification : Specification<Food>
   {
       // Complex filtering with category, keyword, availability
   }
   ```

2. **Custom Ordering**: Override `ApplyOrdering` method
   ```csharp
   public override IQueryable<Food> ApplyOrdering(IQueryable<Food> queryable)
   {
       return queryable.OrderByDescending(f => f.CreatedAt);
   }
   ```

3. **Value Object Handling**: Working with value objects that need special handling
   ```csharp
   public class UserByEmailSpecification : Specification<User>
   {
       // Handles Email value object conversion
   }
   ```

4. **Reusability**: Query logic used across multiple handlers
   ```csharp
   // Used in multiple handlers
   var spec = new FoodsFilterSpecification(category, keyword, isAvailable);
   ```

5. **Business Logic Encapsulation**: Query represents a domain concept
   ```csharp
   public class ActiveCartItemsSpecification : Specification<CartItem>
   {
       // Encapsulates "active cart items" business concept
   }
   ```

### Summary

- **Use `FilterSpecification<T>`** for simple, ad-hoc filters
- **Create specific classes** for complex logic, custom ordering, or reusable queries
- **Use string-based includes** for nested navigation through collections
- **Use expression-based includes** for direct navigation properties
- **Compose specifications** using `And()`, `Or()`, and `Not()` for complex queries
- **Keep specifications in Application layer** in the same feature folder as the handler that uses them

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

### Breaking Down Large Handlers

**✅ DO: Split Handle Method into Smaller, Focused Methods**

When the `Handle` method becomes large or complex, break it down into smaller methods, each with a single responsibility:

```csharp
public class AddCartItemHandler : IRequestHandler<AddCartItemRequest, AddCartItemResponse>
{
    private AddCartItemRequest? _request;

    public async Task<AddCartItemResponse> Handle(
        AddCartItemRequest request,
        CancellationToken cancellationToken)
    {
        _request = request;

        var food = await ValidateAndFetchFoodAsync();
        var existingCartItem = await GetExistingCartItemAsync();

        if (existingCartItem != null)
        {
            return await UpdateExistingCartItemAsync(existingCartItem, food);
        }

        return await CreateNewCartItemAsync(food);
    }

    private async Task<Domain.Food.Food> ValidateAndFetchFoodAsync()
    {
        // Single responsibility: validate and fetch food
    }

    private async Task<CartItem?> GetExistingCartItemAsync()
    {
        // Single responsibility: fetch existing cart item
    }

    private async Task<AddCartItemResponse> UpdateExistingCartItemAsync(
        CartItem existingCartItem,
        Domain.Food.Food food)
    {
        // Single responsibility: update existing cart item
    }

    private async Task<AddCartItemResponse> CreateNewCartItemAsync(Domain.Food.Food food)
    {
        // Single responsibility: create new cart item
    }
}
```

**❌ DON'T: Create Monolithic Handle Methods**

```csharp
// Avoid this pattern - too many responsibilities in one method
public async Task<AddCartItemResponse> Handle(...)
{
    // 50+ lines of mixed validation, fetching, updating, creating, mapping
    // Difficult to test, understand, and maintain
}
```

### Using Private Request Fields

**✅ DO: Store Request in Private Field for Reuse**

Instead of passing request parameters to every method, store the request in a private field:

```csharp
public class AddCartItemHandler : IRequestHandler<AddCartItemRequest, AddCartItemResponse>
{
    private AddCartItemRequest? _request;

    public async Task<AddCartItemResponse> Handle(
        AddCartItemRequest request,
        CancellationToken cancellationToken)
    {
        _request = request; // Store for reuse

        var food = await ValidateAndFetchFoodAsync(); // Uses _request internally
        // ...
    }

    private async Task<Domain.Food.Food> ValidateAndFetchFoodAsync()
    {
        var foodSpecification = new FilterSpecification<Domain.Food.Food>(
            f => f.Id == _request!.FoodId);
        // ...
    }
}
```

**❌ DON'T: Pass Request to Every Method**

```csharp
// Avoid passing request to every method
private async Task<Domain.Food.Food> ValidateAndFetchFoodAsync(
    AddCartItemRequest request) // Unnecessary parameter
{
    // ...
}

private async Task<CartItem?> GetExistingCartItemAsync(
    AddCartItemRequest request) // Unnecessary parameter
{
    // ...
}
```

### Method Naming and Single Responsibility

**✅ DO: Use Descriptive Method Names with Single Responsibility**

Each private method should have a clear, descriptive name that indicates its single responsibility:

```csharp
// Good: Clear, single responsibility
private async Task<Domain.Food.Food> ValidateAndFetchFoodAsync()
private async Task<CartItem?> GetExistingCartItemAsync()
private async Task<AddCartItemResponse> UpdateExistingCartItemAsync(...)
private async Task<AddCartItemResponse> CreateNewCartItemAsync(...)
```

**✅ DO: Keep Methods Focused on One Task**

```csharp
// Good: Method does one thing
private async Task ClearUserCartAsync()
{
    await _cartItemRepository.ClearCart(_request!.UserId);
    _logger.LogInformation("Cleared existing cart items for user {UserId}", _request.UserId);
}

// Good: Method fetches and returns data
private async Task<Dictionary<Guid, Domain.Food.Food>> FetchFoodsAsync()
{
    var foodIds = _request!.Items.Select(i => i.FoodId).Distinct().ToList();
    var foodSpecification = new FilterSpecification<Domain.Food.Food>(
        f => foodIds.Contains(f.Id))
        .Include(f => f.Images.OrderBy(i => i.DisplayOrder).Take(1));
    var foods = await _foodRepository.FilterByExpressionAsync(foodSpecification);
    return foods.ToDictionary(f => f.Id);
}
```

**❌ DON'T: Create Methods with Multiple Responsibilities**

```csharp
// Bad: Method does too many things
private async Task ProcessEverythingAsync()
{
    // Validates, fetches, creates, updates, maps - too many responsibilities
}
```

### Performance Optimization

**✅ DO: Batch Database Operations**

Reduce database round trips by batching operations:

```csharp
// Good: Batch fetch all foods at once
private async Task<Dictionary<Guid, Domain.Food.Food>> FetchFoodsAsync()
{
    var foodIds = _request!.Items.Select(i => i.FoodId).Distinct().ToList();
    var foodSpecification = new FilterSpecification<Domain.Food.Food>(
        f => foodIds.Contains(f.Id))
        .Include(f => f.Images.OrderBy(i => i.DisplayOrder).Take(1));
    var foods = await _foodRepository.FilterByExpressionAsync(foodSpecification);
    return foods.ToDictionary(f => f.Id);
}
```

**✅ DO: Use Filtered Includes to Load Only Required Data**

```csharp
// Good: Load only first image instead of all images
var foodSpecification = new FilterSpecification<Domain.Food.Food>(f => f.Id == foodId)
    .Include(f => f.Images.OrderBy(i => i.DisplayOrder).Take(1));
```

**✅ DO: Avoid Redundant Database Queries**

```csharp
// Good: Fetch food with images once, reuse for validation and mapping
var food = await ValidateAndFetchFoodAsync(); // Includes images
// Use same food entity for validation and response mapping
```

**❌ DON'T: Make Multiple Queries for the Same Data**

```csharp
// Bad: Fetches food twice
var food = await _foodRepository.FindByExpressionAsync(...); // Without images
// ... validation ...
var foodWithImages = await _foodRepository.FindByExpressionAsync(...); // With images
```

### Transaction Management

**✅ DO: Use UnitOfWork for Multi-Table Operations**

When operations involve multiple tables, wrap them in a transaction:

```csharp
public async Task<AddBulkCartItemsResponse> Handle(...)
{
    return await _unitOfWork.ExecuteInTransactionAsync(async () =>
    {
        await ClearUserCartAsync();
        var foodDictionary = await FetchFoodsAsync();
        var addedCartItems = AddCartItems(foodDictionary);
        var items = MapToResponse(addedCartItems);
        return new AddBulkCartItemsResponse { Items = items };
    });
}
```

**✅ DO: Defer Database Operations When Possible**

For operations that can be deferred until `SaveChangesAsync`, mark entities for deletion rather than executing immediately:

```csharp
// Good: Deferrable deletion
public async Task ClearCart(Guid userId)
{
    var specification = new FilterSpecification<CartItem>(c => c.UserId == userId);
    var cartItems = await FilterByExpressionAsync(specification);
    foreach (var cartItem in cartItems)
    {
        Remove(cartItem); // Marks for deletion, executed on SaveChangesAsync
    }
}
```

### Mapping Logic

**✅ DO: Inline Simple Mapping Logic**

For simple, one-off mappings, inline the logic directly:

```csharp
// Good: Simple mapping inlined
return new AddCartItemResponse
{
    Id = cartItem.Id,
    FoodId = cartItem.FoodId,
    Name = food.Name,
    // ... other properties
};
```

**✅ DO: Extract Reusable Mapping Helpers**

For complex or reusable mapping logic, extract helper methods:

```csharp
// Good: Reusable helper for image URL
private string GetImageUrl(IReadOnlyCollection<Domain.Food.FoodImage> images)
{
    var firstImage = images.OrderBy(img => img.DisplayOrder).FirstOrDefault();
    return firstImage == null ? string.Empty : _storageService.GetPublicUrl(firstImage.FileKey);
}
```

**❌ DON'T: Create Mapper Classes for Simple Mappings**

```csharp
// Avoid: Over-engineering for simple mappings
public static class CartMapper
{
    public static CartItemResponse ToResponse(CartItem cartItem, IStorageService storageService)
    {
        // Simple mapping that's only used once
    }
}
```

### Code Comments

**✅ DO: Let Code Speak for Itself**

Write self-documenting code with clear method names and structure:

```csharp
// Good: Method name is self-explanatory
private async Task<Domain.Food.Food> ValidateAndFetchFoodAsync()
{
    // Implementation is clear without comments
}
```

**❌ DON'T: Add Redundant Comments**

```csharp
// Bad: Comment adds no value
// Validate food
var food = await ValidateAndFetchFoodAsync();

// Bad: Comment states the obvious
// If cart item exists, update it
if (existingCartItem != null)
{
    // ...
}
```

### Summary

- **Break down large handlers** into smaller, focused methods
- **Use private request fields** instead of passing request parameters
- **Each method should have a single responsibility**
- **Batch database operations** to reduce round trips
- **Use filtered includes** to load only required data
- **Wrap multi-table operations** in transactions using UnitOfWork
- **Inline simple mappings**, extract complex or reusable logic
- **Write self-documenting code** without redundant comments

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
Application/
├── Cart/
│   ├── GetCartItems/
│   │   ├── GetCartItemsHandler.cs
│   │   ├── GetCartItemsRequest.cs
│   │   ├── GetCartItemsResponse.cs
│   │   └── GetCartItemsSpecification.cs
│   └── ...
├── Food/
│   ├── GetFoods/
│   │   ├── GetFoodsHandler.cs
│   │   ├── GetFoodsRequest.cs
│   │   ├── GetFoodsResponse.cs
│   │   └── GetFoodsSpecification.cs
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

