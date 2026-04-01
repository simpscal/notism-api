using FluentValidation;

namespace Notism.Application.Cart.UpdateCartItemQuantity;

public class UpdateCartItemQuantityRequestValidator : AbstractValidator<UpdateCartItemQuantityRequest>
{
    public UpdateCartItemQuantityRequestValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("UserId is required");

        RuleFor(x => x.CartItemId)
            .NotEmpty()
            .WithMessage("CartItemId is required");

        RuleFor(x => x.Quantity)
            .GreaterThan(0)
            .WithMessage("Quantity must be greater than zero");
    }
}