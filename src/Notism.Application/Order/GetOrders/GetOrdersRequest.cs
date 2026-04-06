using MediatR;

namespace Notism.Application.Order.GetOrders;

public class GetOrdersRequest : IRequest<GetOrdersResponse>
{
    public Guid UserId { get; set; }
    public string? PaymentStatus { get; set; }
}