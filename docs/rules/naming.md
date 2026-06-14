# Naming Conventions

## Naming Conventions

### Aggregate Roots and Entities

**✅ DO: Use Singular Nouns**

```csharp
public class CartItem : AggregateRoot { }
public class Food : AggregateRoot { }
public class User : AggregateRoot { }
```

### Read Projections

**✅ DO: Name a Read Projection Type After Its Read + "Projection"**

A read that shapes its result with `.Select(...)` into a dedicated type names that type
after the read it serves:

```csharp
FoodListProjection
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

### CQRS Feature Folder

**✅ DO: Place Each Operation in `src/Notism.Application/{Feature}/{Operation}/`**

| File | Pattern |
|------|---------|
| Request | `{Operation}Request.cs` |
| Response | `{Operation}Response.cs` |
| Handler | `{Operation}Handler.cs` |
| Validator | `{Operation}RequestValidator.cs` |

Validators and AutoMapper profiles auto-register — no manual DI registration needed.

### Test Naming

**✅ DO: Use `Handle_When{Condition}_{ExpectedOutcome}` for Handler Test Methods**

File location: `tests/Notism.Application.Tests/{Feature}/{Operation}/{Operation}HandlerTests.cs`

---

