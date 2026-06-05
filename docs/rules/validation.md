# Validation

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

