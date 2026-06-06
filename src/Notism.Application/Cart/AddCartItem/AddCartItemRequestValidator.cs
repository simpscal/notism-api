using FluentValidation;

using Notism.Application.Common.Services;

namespace Notism.Application.Cart.AddCartItem;

public class AddCartItemRequestValidator : AbstractValidator<AddCartItemRequest>
{
    public AddCartItemRequestValidator(IMessages messages)
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage(messages.UserIdRequired);

        RuleFor(x => x.FoodId)
            .NotEmpty()
            .WithMessage(messages.FoodIdRequired);

        RuleFor(x => x.Quantity)
            .GreaterThan(0)
            .WithMessage(messages.QuantityMustBeGreaterThanZero);
    }
}