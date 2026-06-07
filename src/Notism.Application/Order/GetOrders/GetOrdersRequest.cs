using MediatR;

using Notism.Shared.Models;

namespace Notism.Application.Order.GetOrders;

public record GetOrdersRequest : Pagination, IRequest<GetOrdersResponse>
{
    public Guid UserId { get; set; }
    public string? PaymentStatus { get; set; }
}