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

### Endpoint Payloads

**✅ DO: Define Each Inbound Payload in Its Own File**

No payload/request records live inside endpoint files. Each inbound payload is a `record` in its own file under `src/Notism.Api/Models/Payloads/`, named after the type (StyleCop SA1649: one public type per file, filename = type name). Endpoint files contain only the static endpoint-mapping class.

**✅ DO: Name the API-Surface Input `*Payload`**

The API-surface input record is a `*Payload`; the endpoint maps it to the corresponding Application `*Request` before dispatching via `ISender`.

### Interface-Necessity Rule

**✅ DO: Introduce an Interface Only When It Earns Its Keep**

An interface exists only when it **crosses a layer boundary** (e.g. an Application-defined service implemented in Infrastructure) or **backs an actively used test seam**. A same-layer service with a single implementation, a single consumer, and no test double should be injected concretely rather than behind a redundant interface.

**✅ DO: Place Interfaces by Their Role**

Interfaces live in `Common/Interfaces` within their layer, or beside the aggregate in the Domain layer (repository interfaces).

---

