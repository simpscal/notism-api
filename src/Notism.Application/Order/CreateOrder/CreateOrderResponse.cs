using Notism.Shared.Extensions;

namespace Notism.Application.Order.CreateOrder;

public class CreateOrderResponse
{
    public Guid OrderId { get; set; }
    public string SlugId { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public string DeliveryStatus { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }

    public static CreateOrderResponse FromDomain(Domain.Order.Order order)
    {
        return new CreateOrderResponse
        {
            OrderId = order.Id,
            SlugId = order.SlugId,
            TotalAmount = order.TotalAmount,
            PaymentMethod = order.PaymentMethod.GetStringValue(),
            DeliveryStatus = order.DeliveryStatus.GetStringValue(),
            CreatedAt = order.CreatedAt,
        };
    }
}
