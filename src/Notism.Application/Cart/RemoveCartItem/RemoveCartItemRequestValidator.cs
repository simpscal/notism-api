using FluentValidation;

namespace Notism.Application.Cart.RemoveCartItem;

public class RemoveCartItemRequestValidator : AbstractValidator<RemoveCartItemRequest>
{
    public RemoveCartItemRequestValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("UserId is required");

        RuleFor(x => x.CartItemId)
            .NotEmpty()
            .WithMessage("CartItemId is required");
    }
}