using MediatR;

using Notism.Application.Order.Common;

namespace Notism.Application.Order.ApproveRefund;

public class ApproveRefundRequest : IRequest<RefundResponse>
{
    public Guid RefundId { get; set; }
}