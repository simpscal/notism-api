using MediatR;

namespace Notism.Application.Payment.HandleSepayWebhook;

public class HandleSepayWebhookRequest : IRequest
{
    public string TransactionId { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Description { get; set; } = string.Empty;
    public DateTime TransferredAt { get; set; }
}
