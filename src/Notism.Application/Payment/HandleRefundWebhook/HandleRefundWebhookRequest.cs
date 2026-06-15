using MediatR;

namespace Notism.Application.Payment.HandleRefundWebhook;

public class HandleRefundWebhookRequest : IRequest
{
    public string Secret { get; set; } = string.Empty;
    public string TransferReference { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? FailureReason { get; set; }
}