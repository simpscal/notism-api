using Notism.Shared.Extensions;

namespace Notism.Application.Order.AdminUpdateOrderDeliveryStatus;

public sealed record AdminUpdateOrderDeliveryStatusResponse
{
    public Guid Id { get; set; }
    public string SlugId { get; set; } = string.Empty;
    public Guid UserId { get; set; }
    public string UserEmail { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public string DeliveryStatus { get; set; } = string.Empty;
    public string PaymentStatus { get; set; } = string.Empty;
    public DateTime? PaidAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public int TotalItems { get; set; }
    public string? DeliveryNotes { get; set; }

    public static AdminUpdateOrderDeliveryStatusResponse FromDomain(Domain.Order.Order order)
    {
        var user = order.User;

        return new AdminUpdateOrderDeliveryStatusResponse
        {
            Id = order.Id,
            SlugId = order.SlugId,
            UserId = order.UserId,
            UserEmail = user?.Email.Value ?? string.Empty,
            UserName = user?.FullName ?? string.Empty,
            TotalAmount = order.TotalAmount,
            DeliveryStatus = order.DeliveryStatus.GetStringValue(),
            PaymentStatus = order.PaymentStatus.GetStringValue(),
            PaidAt = order.PaidAt,
            CreatedAt = order.CreatedAt,
            UpdatedAt = order.UpdatedAt,
            TotalItems = order.Items.Count,
            DeliveryNotes = order.DeliveryNotes,
        };
    }
}