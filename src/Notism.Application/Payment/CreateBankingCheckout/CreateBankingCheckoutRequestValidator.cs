using FluentValidation;

namespace Notism.Application.Payment.CreateBankingCheckout;

public class CreateBankingCheckoutRequestValidator : AbstractValidator<CreateBankingCheckoutRequest>
{
    public CreateBankingCheckoutRequestValidator()
    {
        RuleFor(x => x.CartItemIds)
            .NotNull()
            .WithMessage("Cart item IDs are required")
            .NotEmpty()
            .WithMessage("At least one cart item ID is required");

        RuleForEach(x => x.CartItemIds)
            .NotEmpty()
            .WithMessage("Cart item ID cannot be empty");

        RuleFor(x => x.TotalAmount)
            .GreaterThan(0)
            .WithMessage("Total amount must be greater than 0");
    }
}
