using MediatR;

namespace Notism.Application.Order.GetRefundById;

public class GetRefundByIdRequest : IRequest<GetRefundByIdResponse>
{
    public Guid RefundId { get; set; }
}