using Notism.Domain.Common;
using Notism.Domain.Order.Enums;

namespace Notism.Domain.Order;

public class Refund : Entity
{
    public Guid OrderId { get; private set; }
    public decimal Amount { get; private set; }
    public RefundStatus Status { get; private set; }
    public string? TransferReference { get; private set; }
    public string? FailureReason { get; private set; }
    public DateTime? PaidAt { get; private set; }
    public Order Order { get; private set; } = null!;

    private Refund(Guid orderId, decimal amount)
    {
        OrderId = orderId;
        Amount = amount;
        Status = RefundStatus.Pending;
    }

    internal static Refund Create(Guid orderId, decimal amount)
    {
        if (amount <= 0)
        {
            throw new ArgumentException("Refund amount must be greater than zero", nameof(amount));
        }

        return new Refund(orderId, amount);
    }

    internal void MarkProcessing(string transferReference)
    {
        if (Status != RefundStatus.Pending && Status != RefundStatus.Failed)
        {
            throw new InvalidOperationException($"Refund cannot transition from {Status} to Processing");
        }

        Status = RefundStatus.Processing;
        TransferReference = transferReference;
        FailureReason = null;
        UpdatedAt = DateTime.UtcNow;
    }

    internal void MarkPaid()
    {
        if (Status != RefundStatus.Processing)
        {
            throw new InvalidOperationException($"Refund cannot transition from {Status} to Paid");
        }

        Status = RefundStatus.Paid;
        PaidAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    internal void MarkFailed(string reason)
    {
        if (Status != RefundStatus.Pending && Status != RefundStatus.Processing)
        {
            throw new InvalidOperationException($"Refund cannot transition from {Status} to Failed");
        }

        Status = RefundStatus.Failed;
        FailureReason = reason;
        UpdatedAt = DateTime.UtcNow;
    }

    private Refund()
    {
    }
}