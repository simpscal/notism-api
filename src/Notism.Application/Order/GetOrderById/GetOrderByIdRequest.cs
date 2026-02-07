using MediatR;

namespace Notism.Application.Order.GetOrderById;

public class GetOrderByIdRequest : IRequest<GetOrderByIdResponse>
{
    public Guid OrderId { get; set; }
    public Guid UserId { get; set; }
}