using Notism.Shared.Extensions;

using DomainOrder = Notism.Domain.Order.Order;

namespace Notism.Application.Order.GetRefundById;

public sealed record GetRefundByIdResponse
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    public string OrderSlugId { get; set; } = string.Empty;
    public Guid UserId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? TransferReference { get; set; }
    public string? FailureReason { get; set; }
    public DateTime? PaidAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public static GetRefundByIdResponse FromDomain(DomainOrder order)
    {
        var refund = order.Refund!;

        return new GetRefundByIdResponse
        {
            Id = refund.Id,
            OrderId = refund.OrderId,
            OrderSlugId = order.SlugId,
            UserId = order.UserId,
            CustomerName = order.User?.FullName ?? string.Empty,
            CustomerEmail = order.User?.Email.Value ?? string.Empty,
            Amount = refund.Amount,
            Status = refund.Status.GetStringValue(),
            TransferReference = refund.TransferReference,
            FailureReason = refund.FailureReason,
            PaidAt = refund.PaidAt,
            CreatedAt = refund.CreatedAt,
            UpdatedAt = refund.UpdatedAt,
        };
    }
}