using MediatR;

namespace Notism.Application.Order.AdminUpdateOrderDeliveryStatus;

public class AdminUpdateOrderDeliveryStatusRequest : IRequest<AdminUpdateOrderDeliveryStatusResponse>
{
    public Guid OrderId { get; set; }
    public string DeliveryStatus { get; set; } = string.Empty;
}