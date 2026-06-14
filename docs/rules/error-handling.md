# Error Handling

## Error Handling

### Use ResultFailureException

**✅ DO: Throw ResultFailureException for Business Violations**

```csharp
var food = await _readDbContext.Set<Domain.Food.Food>()
    .Where(f => f.Id == request.FoodId)
    .FirstOrDefaultAsync(cancellationToken)
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
var food = await _readDbContext.Set<Domain.Food.Food>()
    .Where(f => f.Id == request.FoodId).FirstOrDefaultAsync(cancellationToken);
if (food == null)
{
    return new AddCartItemResponse { Success = false, ErrorCode = 404 };
}
```

### Exception → HTTP

| Exception | HTTP |
|-----------|------|
| `ResultFailureException` | 400 |
| `NotFoundException` | 404 |
| `UnauthorizedException` | 401 |
| `ForbiddenException` | 403 |
| `InvalidRefreshTokenException` | 401 |

Never return null/bool for failures — throw.

---

