using Notism.Domain.Order;
using Notism.Domain.Order.Enums;
using Notism.Shared.Extensions;

namespace Notism.Application.Order.Common;

public sealed record OrderRefundResponse
{
    public Guid Id { get; set; }
    public decimal Amount { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public static OrderRefundResponse FromDomain(Refund refund)
    {
        return new OrderRefundResponse
        {
            Id = refund.Id,
            Amount = refund.Amount,
            Status = CustomerVisibleStatus(refund.Status).GetStringValue(),
            CreatedAt = refund.CreatedAt,
            UpdatedAt = refund.UpdatedAt,
        };
    }

    private static RefundStatus CustomerVisibleStatus(RefundStatus status)
        => status is RefundStatus.Processing or RefundStatus.Failed ? RefundStatus.Pending : status;
}