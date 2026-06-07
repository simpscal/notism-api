using Notism.Application.Common.Interfaces;
using Notism.Application.Order.Common;
using Notism.Shared.Extensions;
using Notism.Shared.Models;

namespace Notism.Application.Order.GetOrders;

public sealed record GetOrdersResponse : PagedResult<GetOrdersOrderResponse>
{
    public static GetOrdersResponse FromDomain(
        PagedResult<Domain.Order.Order> pagedOrders,
        IStorageService storageService)
    {
        return new GetOrdersResponse
        {
            TotalCount = pagedOrders.TotalCount,
            Items = pagedOrders.Items
                .Select(order => GetOrdersOrderResponse.FromDomain(order, storageService))
                .ToList(),
        };
    }
}

public sealed record GetOrdersOrderResponse
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

    public static GetOrdersOrderResponse FromDomain(
        Domain.Order.Order order,
        IStorageService storageService)
    {
        return new GetOrdersOrderResponse
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
            DeliveryNotes = order.DeliveryNotes,
        };
    }
}