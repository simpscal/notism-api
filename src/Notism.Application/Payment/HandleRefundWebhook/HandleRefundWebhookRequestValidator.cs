using FluentValidation;

namespace Notism.Application.Payment.HandleRefundWebhook;

public class HandleRefundWebhookRequestValidator : AbstractValidator<HandleRefundWebhookRequest>
{
    public HandleRefundWebhookRequestValidator()
    {
        RuleFor(x => x.TransferReference).NotEmpty();
        RuleFor(x => x.Status).NotEmpty();
    }
}