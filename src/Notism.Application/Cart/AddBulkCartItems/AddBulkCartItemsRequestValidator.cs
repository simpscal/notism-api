using FluentValidation;

namespace Notism.Application.Cart.AddBulkCartItems;

public class AddBulkCartItemsRequestValidator : AbstractValidator<AddBulkCartItemsRequest>
{
    public AddBulkCartItemsRequestValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("UserId is required");

        RuleFor(x => x.Items)
            .NotEmpty()
            .WithMessage("At least one item is required");

        RuleForEach(x => x.Items)
            .SetValidator(new CartItemRequestValidator());
    }
}

public class CartItemRequestValidator : AbstractValidator<CartItemRequest>
{
    public CartItemRequestValidator()
    {
        RuleFor(x => x.FoodId)
            .NotEmpty()
            .WithMessage("FoodId is required");

        RuleFor(x => x.Quantity)
            .GreaterThan(0)
            .WithMessage("Quantity must be greater than zero");
    }
}

