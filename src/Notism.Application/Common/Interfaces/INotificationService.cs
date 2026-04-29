namespace Notism.Application.Common.Interfaces;

public interface INotificationService
{
    Task NotifyPaymentSuccessAsync(Guid orderId, Guid userId, DateTime paidAt, CancellationToken cancellationToken);
    Task NotifyPaymentFailureAsync(Guid orderId, Guid userId, CancellationToken cancellationToken);
}
