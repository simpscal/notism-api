using Notism.Application.Common.Services;
using Notism.Application.Order.Common;
using Notism.Shared.Extensions;

namespace Notism.Application.Order.GetOrderById;

public sealed record GetOrderByIdResponse
{
    public Guid Id { get; set; }
    public string SlugId { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public string DeliveryStatus { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public List<OrderItemResponse> Items { get; set; } = new();
    public DeliveryStatusTimingResponse DeliveryStatusTiming { get; set; } = new();
    public string PaymentStatus { get; set; } = string.Empty;
    public DateTime? PaidAt { get; set; }
    public PaymentQrResponse? PaymentQr { get; set; }
    public string? DeliveryNotes { get; set; }
    public OrderRefundResponse? Refund { get; set; }

    public static GetOrderByIdResponse FromDomain(
        Domain.Order.Order order,
        IStorageService storageService,
        PaymentQrResponse? paymentQr,
        bool hasBankDetails)
    {
        return new GetOrderByIdResponse
        {
            Id = order.Id,
            SlugId = order.SlugId,
            TotalAmount = order.TotalAmount,
            PaymentMethod = order.PaymentMethod.GetStringValue(),
            DeliveryStatus = order.DeliveryStatus.GetStringValue(),
            CreatedAt = order.CreatedAt,
            UpdatedAt = order.UpdatedAt,
            Items = order.Items.Select(item => OrderItemResponse.FromDomain(item, storageService)).ToList(),
            DeliveryStatusTiming = DeliveryStatusTimingResponse.FromDomain(order.GetDeliveryStatusTiming()),
            PaymentStatus = order.PaymentStatus.GetStringValue(),
            PaidAt = order.PaidAt,
            PaymentQr = paymentQr,
            DeliveryNotes = order.DeliveryNotes,
            Refund = order.Refund == null ? null : OrderRefundResponse.FromDomain(order.Refund, hasBankDetails),
        };
    }
}