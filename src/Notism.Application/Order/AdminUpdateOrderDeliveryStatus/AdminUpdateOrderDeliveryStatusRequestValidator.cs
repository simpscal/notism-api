using FluentValidation;

using Notism.Domain.Order.Enums;
using Notism.Shared.Extensions;

namespace Notism.Application.Order.AdminUpdateOrderDeliveryStatus;

public class AdminUpdateOrderDeliveryStatusRequestValidator : AbstractValidator<AdminUpdateOrderDeliveryStatusRequest>
{
    public AdminUpdateOrderDeliveryStatusRequestValidator()
    {
        RuleFor(x => x.OrderId)
            .NotEmpty()
            .WithMessage("Order ID is required");

        RuleFor(x => x.DeliveryStatus)
            .NotEmpty()
            .WithMessage("Delivery status is required")
            .Must(status => status.ExistInEnum<DeliveryStatus>())
            .WithMessage("Delivery status must be a valid delivery status");
    }
}