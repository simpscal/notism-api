using Notism.Domain.Order;
using Notism.Shared.Extensions;
using Notism.Shared.Models;

namespace Notism.Application.Order.AdminRefundsForTable;

public sealed record AdminRefundsForTableResponse : PagedResult<AdminRefundsForTableRefundResponse>;

public sealed record AdminRefundsForTableRefundResponse
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    public string OrderReference { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? TransferReference { get; set; }
    public string? FailureReason { get; set; }
    public DateTime? PaidAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public static AdminRefundsForTableRefundResponse FromDomain(Refund refund)
    {
        return new AdminRefundsForTableRefundResponse
        {
            Id = refund.Id,
            OrderId = refund.OrderId,
            OrderReference = refund.Order?.SlugId ?? string.Empty,
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