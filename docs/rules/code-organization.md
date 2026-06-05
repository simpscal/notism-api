# Code Organization

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

