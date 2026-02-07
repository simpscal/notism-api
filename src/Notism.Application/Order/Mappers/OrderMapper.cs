using Notism.Application.Common.Interfaces;
using Notism.Application.Order.Models;
using Notism.Domain.Order;
using Notism.Domain.Order.Enums;
using Notism.Shared.Extensions;

namespace Notism.Application.Order.Mappers;

public static class OrderMapper
{
    public static OrderResponse ToResponse(
        Domain.Order.Order order,
        IStorageService storageService)
    {
        return new OrderResponse
        {
            Id = order.Id,
            TotalAmount = order.TotalAmount,
            PaymentMethod = order.PaymentMethod.GetStringValue(),
            DeliveryStatus = order.DeliveryStatus.GetStringValue(),
            CreatedAt = order.CreatedAt,
            UpdatedAt = order.UpdatedAt,
            Items = order.Items.Select(item => ToOrderItemResponse(item, storageService)).ToList(),
            DeliveryStatusTiming = CalculateDeliveryStatusTiming(order),
        };
    }

    public static OrderItemResponse ToOrderItemResponse(
        OrderItem item,
        IStorageService storageService)
    {
        return new OrderItemResponse
        {
            Id = item.Id,
            FoodId = item.FoodId,
            FoodName = item.FoodName,
            UnitPrice = item.UnitPrice,
            DiscountPrice = item.DiscountPrice,
            Quantity = item.Quantity,
            TotalPrice = item.TotalPrice,
            ImageUrl = GetImageUrl(item.Food.Images, storageService),
        };
    }

    private static string GetImageUrl(
        IReadOnlyCollection<Domain.Food.FoodImage> images,
        IStorageService storageService)
    {
        var firstImage = images.OrderBy(img => img.DisplayOrder).FirstOrDefault();
        return firstImage == null ? string.Empty : storageService.GetPublicUrl(firstImage.FileKey);
    }

    private static DeliveryStatusTimingResponse CalculateDeliveryStatusTiming(Domain.Order.Order order)
    {
        var history = order.StatusHistory
            .OrderBy(h => h.StatusChangedAt)
            .ToList();

        var timing = new DeliveryStatusTimingResponse();

        var orderPlacedAt = history.FirstOrDefault(h => h.Status == DeliveryStatus.OrderPlaced)?.StatusChangedAt;
        var preparingAt = history.FirstOrDefault(h => h.Status == DeliveryStatus.Preparing)?.StatusChangedAt;
        var onTheWayAt = history.FirstOrDefault(h => h.Status == DeliveryStatus.OnTheWay)?.StatusChangedAt;
        var deliveredAt = history.FirstOrDefault(h => h.Status == DeliveryStatus.Delivered)?.StatusChangedAt;

        if (preparingAt.HasValue)
        {
            timing.OrderPlacedCompletedAt = preparingAt.Value;
        }

        if (onTheWayAt.HasValue)
        {
            timing.PreparingCompletedAt = onTheWayAt.Value;
        }

        if (deliveredAt.HasValue)
        {
            timing.OnTheWayCompletedAt = deliveredAt.Value;
        }

        if (deliveredAt.HasValue && order.DeliveryStatus == DeliveryStatus.Delivered)
        {
            timing.DeliveredCompletedAt = deliveredAt.Value;
        }

        return timing;
    }
}