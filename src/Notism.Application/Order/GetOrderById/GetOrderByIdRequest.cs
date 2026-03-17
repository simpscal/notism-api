using MediatR;

namespace Notism.Application.Order.GetOrderById;

public class GetOrderByIdRequest : IRequest<GetOrderByIdResponse>
{
    public string SlugId { get; set; } = string.Empty;
    public Guid UserId { get; set; }
    public string Role { get; set; } = string.Empty;
}