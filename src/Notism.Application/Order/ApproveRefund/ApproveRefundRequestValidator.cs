using FluentValidation;

namespace Notism.Application.Order.ApproveRefund;

public class ApproveRefundRequestValidator : AbstractValidator<ApproveRefundRequest>
{
    public ApproveRefundRequestValidator()
    {
        RuleFor(x => x.RefundId)
            .NotEmpty()
            .WithMessage("Refund ID is required");
    }
}