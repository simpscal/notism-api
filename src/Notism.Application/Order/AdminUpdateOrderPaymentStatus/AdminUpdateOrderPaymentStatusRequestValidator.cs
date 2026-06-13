using FluentValidation;

using Notism.Domain.Payment.Enums;
using Notism.Shared.Extensions;

namespace Notism.Application.Order.AdminUpdateOrderPaymentStatus;

public class AdminUpdateOrderPaymentStatusRequestValidator : AbstractValidator<AdminUpdateOrderPaymentStatusRequest>
{
    public AdminUpdateOrderPaymentStatusRequestValidator()
    {
        RuleFor(x => x.OrderId)
            .NotEmpty()
            .WithMessage("Order ID is required");

        RuleFor(x => x.PaymentStatus)
            .NotEmpty()
            .WithMessage("Payment status is required")
            .Must(status => status.ExistInEnum<PaymentStatus>())
            .WithMessage("Payment status must be a valid payment status");
    }
}