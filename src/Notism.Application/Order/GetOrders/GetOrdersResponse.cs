using Notism.Application.Order.Models;

namespace Notism.Application.Order.GetOrders;

public class GetOrdersResponse
{
    public List<OrderResponse> Orders { get; set; } = new();
}