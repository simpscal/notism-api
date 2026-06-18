using FluentValidation;

namespace Notism.Application.Order.RetryRefund;

public class RetryRefundRequestValidator : AbstractValidator<RetryRefundRequest>
{
    public RetryRefundRequestValidator()
    {
        RuleFor(x => x.RefundId)
            .NotEmpty()
            .WithMessage("Refund ID is required");
    }
}