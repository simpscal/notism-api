using FluentValidation;

namespace Notism.Application.Cart.ClearCart;

public class ClearCartRequestValidator : AbstractValidator<ClearCartRequest>
{
    public ClearCartRequestValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("UserId is required");
    }
}

