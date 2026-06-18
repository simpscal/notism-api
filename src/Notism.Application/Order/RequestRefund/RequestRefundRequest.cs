using MediatR;

using Notism.Application.Order.Common;

namespace Notism.Application.Order.RequestRefund;

public class RequestRefundRequest : IRequest<OrderRefundResponse>
{
    public Guid OrderId { get; set; }
    public Guid UserId { get; set; }
}