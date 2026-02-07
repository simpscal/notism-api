using FluentValidation;

using Notism.Domain.Order.Enums;
using Notism.Shared.Extensions;

namespace Notism.Application.Order.CreateOrder;

public class CreateOrderRequestValidator : AbstractValidator<CreateOrderRequest>
{
    public CreateOrderRequestValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("User ID is required");

        RuleFor(x => x.PaymentMethod)
            .NotEmpty()
            .WithMessage("Payment method is required")
            .Must(method => method.ExistInEnum<PaymentMethod>())
            .WithMessage("Payment method must be a valid payment method")
            .Must(method =>
            {
                var paymentMethod = method.ToEnum<PaymentMethod>();
                return paymentMethod == PaymentMethod.CashOnDelivery;
            })
            .WithMessage("Only cash on delivery payment is currently supported");

        RuleFor(x => x.CartItemIds)
            .NotEmpty()
            .WithMessage("At least one cart item ID is required");

        RuleForEach(x => x.CartItemIds)
            .NotEmpty()
            .WithMessage("Cart item ID cannot be empty");
    }
}