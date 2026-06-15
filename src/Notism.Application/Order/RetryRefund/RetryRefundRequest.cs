using MediatR;

using Notism.Application.Order.Common;

namespace Notism.Application.Order.RetryRefund;

public class RetryRefundRequest : IRequest<RefundResponse>
{
    public Guid RefundId { get; set; }
}