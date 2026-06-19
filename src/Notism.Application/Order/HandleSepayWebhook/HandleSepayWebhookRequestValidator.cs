using FluentValidation;

namespace Notism.Application.Order.HandleSepayWebhook;

public class HandleSepayWebhookRequestValidator : AbstractValidator<HandleSepayWebhookRequest>
{
    public HandleSepayWebhookRequestValidator()
    {
        RuleFor(x => x.TransactionId).NotEmpty();
        RuleFor(x => x.Amount).GreaterThan(0);
        RuleFor(x => x.Content).NotEmpty();
    }
}
