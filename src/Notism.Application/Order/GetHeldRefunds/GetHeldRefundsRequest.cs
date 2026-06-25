using MediatR;

namespace Notism.Application.Order.GetHeldRefunds;

public class GetHeldRefundsRequest : IRequest<GetHeldRefundsResponse>
{
    public Guid UserId { get; set; }
}