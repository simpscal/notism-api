namespace Notism.Application.Common.Services;

public interface IPaymentNotifier
{
    Task NotifyPaymentSuccessAsync(Guid orderId, Guid userId, DateTime paidAt, string slugId, CancellationToken cancellationToken);
    Task NotifyPaymentFailureAsync(Guid orderId, Guid userId, CancellationToken cancellationToken);
    Task NotifyRefundPaidAsync(Guid refundId, string orderId, string orderRef, decimal amount, DateTime sentDate, Guid userId, CancellationToken cancellationToken);
    Task NotifyAdminRefundStatusChangedAsync(Guid refundId, string status, CancellationToken cancellationToken);
}