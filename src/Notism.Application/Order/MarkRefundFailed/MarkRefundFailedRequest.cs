using MediatR;

using Notism.Application.Order.Common;

namespace Notism.Application.Order.MarkRefundFailed;

public class MarkRefundFailedRequest : IRequest<RefundResponse>
{
    public Guid RefundId { get; set; }
    public string Reason { get; set; } = string.Empty;
}