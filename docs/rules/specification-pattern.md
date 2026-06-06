# Specification Pattern

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

**✅ DO: Use String-Based Includes for Nested Navigation Properties — but only inside a named specification**

For nested navigation properties (especially through collections), string-based
includes with dot notation are still required because the specification base type
lives in the Domain layer, which does not reference EF Core and therefore cannot
express `ThenInclude`. Such string includes **must be encapsulated inside a named
specification class** (e.g. `OrderDetailSpecification`,
`CartItemDetailSpecification`, `FoodWithCustomisationsByIdSpecification`) and must
never be scattered inline across handlers. Prefer lambda includes for direct
navigation properties.

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

