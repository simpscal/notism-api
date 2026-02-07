using MediatR;

namespace Notism.Application.Order.UpdateDeliveryStatus;

public class UpdateDeliveryStatusRequest : IRequest<UpdateDeliveryStatusResponse>
{
    public Guid OrderId { get; set; }
    public Guid UserId { get; set; }
    public string DeliveryStatus { get; set; } = string.Empty;
}