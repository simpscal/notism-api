using FluentValidation;

namespace Notism.Application.Order.RequestRefund;

public class RequestRefundRequestValidator : AbstractValidator<RequestRefundRequest>
{
    public RequestRefundRequestValidator()
    {
        RuleFor(x => x.OrderId)
            .NotEmpty()
            .WithMessage("Order ID is required");

        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("User ID is required");
    }
}