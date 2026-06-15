using FluentValidation;

namespace Notism.Application.Order.MarkRefundFailed;

public class MarkRefundFailedRequestValidator : AbstractValidator<MarkRefundFailedRequest>
{
    public MarkRefundFailedRequestValidator()
    {
        RuleFor(x => x.RefundId)
            .NotEmpty()
            .WithMessage("Refund ID is required");

        RuleFor(x => x.Reason)
            .NotEmpty()
            .WithMessage("Reason is required");
    }
}