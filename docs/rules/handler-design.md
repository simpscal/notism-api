# Handler Design

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

### Response Models

Handlers map domain entities to dedicated response models. These models follow a single, authoritative shape.

**✅ DO: Declare Every Response as a `sealed record`**

Every response model is a `record`, and it is `sealed` unless it is a base type that other responses inherit. The only permitted non-`sealed` base records are `Food/Common/CategoryResponse` and `Cart/Common/CartItemResponse`, both intentionally left open for inheritance.

**✅ DO: Build Projecting Responses Through a `FromDomain` Factory**

Responses that project a domain entity expose a `static FromDomain(...)` factory and are built only through it — never via ad hoc object initialisers in handlers:

```csharp
return CartItemResponse.FromDomain(cartItem, food);
```

Trivial responses that do not map a domain entity (message acknowledgements, count DTOs, paginated wrappers, pre-signed URLs) are constructed directly and do not need a factory.

**✅ DO: Use `required`, Reserve Nullable for Genuinely Optional Values**

A property is declared nullable (`T?`) only when the value is genuinely optional in the contract. Required reference values use the `required` modifier. Do not paper over optionality with `= string.Empty` defaults on conceptually-required fields.

**✅ DO: Derive Paginated Responses from `PagedResult<T>`**

Paginated responses derive from `Notism.Shared.Models.PagedResult<T>` (`{ TotalCount, Items }`). Do not hand-roll a bare `List<T>` property for a paginated result.

**❌ DON'T: Rename a Response Property Without Treating It as a Breaking Change**

System.Text.Json uses the default camelCase policy, so the serialized field name is derived from the C# property name. Never rename a response property (or change its `JsonPropertyName`) without treating it as a breaking API change.

### Summary

- **Break down large handlers** into smaller, focused methods
- **Use private request fields** instead of passing request parameters
- **Each method should have a single responsibility**
- **Batch database operations** to reduce round trips
- **Use filtered includes** to load only required data
- **Wrap multi-table operations** in transactions using UnitOfWork
- **Inline simple mappings**, extract complex or reusable logic
- **Write self-documenting code** without redundant comments
- **Model responses as `sealed record`s** built through `FromDomain` factories, using `required` over nullable defaults

---

