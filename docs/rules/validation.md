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

### Localized Validation Messages

**✅ DO: Source Validation Messages from `IMessages`**

Validation messages source from `IMessages` (the localized message provider) rather than hard-coded English string literals, so that validator output matches the localization already used by handlers. New validators must inject `IMessages` and reference message keys:

```csharp
public class CreateEntityRequestValidator : AbstractValidator<CreateEntityRequest>
{
    public CreateEntityRequestValidator(IMessages messages)
    {
        RuleFor(x => x.Quantity)
            .GreaterThan(0)
            .WithMessage(messages.ValueMustBePositive);
    }
}
```

### Validation Failure Behavior

`ValidationBehavior` throws a validation-specific exception (`Notism.Shared.Exceptions.ValidationException`), not `ResultFailureException`. `ResultFailureMiddleware` maps it to HTTP 400. The serialized error body is unchanged (joined message text under `message`).

---

