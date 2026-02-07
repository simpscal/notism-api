using FluentValidation;

using Notism.Domain.Order.Enums;
using Notism.Shared.Extensions;

namespace Notism.Application.Order.UpdateDeliveryStatus;

public class UpdateDeliveryStatusRequestValidator : AbstractValidator<UpdateDeliveryStatusRequest>
{
    public UpdateDeliveryStatusRequestValidator()
    {
        RuleFor(x => x.OrderId)
            .NotEmpty()
            .WithMessage("Order ID is required");

        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("User ID is required");

        RuleFor(x => x.DeliveryStatus)
            .NotEmpty()
            .WithMessage("Delivery status is required")
            .Must(status => status.ExistInEnum<DeliveryStatus>())
            .WithMessage("Delivery status must be a valid delivery status");
    }
}