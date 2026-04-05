using FluentValidation;

namespace Notism.Application.Payment.HandleSepayWebhook;

public class HandleSepayWebhookRequestValidator : AbstractValidator<HandleSepayWebhookRequest>
{
    public HandleSepayWebhookRequestValidator()
    {
        RuleFor(x => x.TransactionId).NotEmpty();
        RuleFor(x => x.Amount).GreaterThan(0);
        RuleFor(x => x.Description).NotEmpty();
    }
}
