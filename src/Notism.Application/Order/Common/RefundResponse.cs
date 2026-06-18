using Notism.Domain.Order;
using Notism.Shared.Extensions;

namespace Notism.Application.Order.Common;

public sealed record RefundResponse
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    public decimal Amount { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? TransferReference { get; set; }
    public string? FailureReason { get; set; }
    public DateTime? PaidAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public static RefundResponse FromDomain(Refund refund)
    {
        return new RefundResponse
        {
            Id = refund.Id,
            OrderId = refund.OrderId,
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