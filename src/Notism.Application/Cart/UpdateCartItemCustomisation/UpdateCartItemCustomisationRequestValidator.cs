using FluentValidation;

namespace Notism.Application.Cart.UpdateCartItemCustomisation;

public class UpdateCartItemCustomisationRequestValidator : AbstractValidator<UpdateCartItemCustomisationRequest>
{
    public UpdateCartItemCustomisationRequestValidator()
    {
        RuleFor(x => x.CartItemId)
            .NotEmpty()
            .WithMessage("CartItemId is required");

        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("UserId is required");

        RuleFor(x => x.CustomisationOptionId)
            .NotEmpty()
            .WithMessage("CustomisationOptionId is required");
    }
}
