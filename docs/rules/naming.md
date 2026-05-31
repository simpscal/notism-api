# Naming Conventions

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

