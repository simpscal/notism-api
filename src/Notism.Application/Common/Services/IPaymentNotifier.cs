namespace Notism.Application.Common.Services;

public interface IPaymentNotifier
{
    Task NotifyPaymentSuccessAsync(Guid orderId, Guid userId, DateTime paidAt, string slugId, CancellationToken cancellationToken);
    Task NotifyPaymentFailureAsync(Guid orderId, Guid userId, CancellationToken cancellationToken);

    /// <summary>
    /// Pushes a refund status change on the shared payment channel to both audiences: always the
    /// admins group (live ledger/detail), and the owning customer only when <paramref name="status"/>
    /// is "paid" (customers are never notified of a failed refund). The customer-facing fields
    /// (<paramref name="orderRef"/>, <paramref name="amount"/>, <paramref name="sentDate"/>) are used
    /// only for the "paid" push.
    /// </summary>
    Task NotifyRefundStatusChangedAsync(Guid refundId, string status, Guid customerUserId, string orderRef, decimal amount, DateTime? sentDate, CancellationToken cancellationToken);
}