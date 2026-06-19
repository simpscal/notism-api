using MediatR;

namespace Notism.Application.Order.HandleSepayWebhook;

public class HandleSepayWebhookRequest : IRequest
{
    public string TransactionId { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Content { get; set; } = string.Empty;
    public string TransferType { get; set; } = string.Empty;
    public DateTime TransferredAt { get; set; }
}
