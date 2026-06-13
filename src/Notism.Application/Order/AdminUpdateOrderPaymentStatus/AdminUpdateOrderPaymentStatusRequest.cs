using MediatR;

namespace Notism.Application.Order.AdminUpdateOrderPaymentStatus;

public class AdminUpdateOrderPaymentStatusRequest : IRequest<AdminUpdateOrderPaymentStatusResponse>
{
    public Guid OrderId { get; set; }
    public string PaymentStatus { get; set; } = string.Empty;
}