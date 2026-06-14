using MediatR;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using Notism.Application.Common.Persistence;
using Notism.Application.Common.Services;
using Notism.Domain.Order.Repositories;
using Notism.Domain.Payment.Enums;
using Notism.Shared.Exceptions;
using Notism.Shared.Extensions;

using DomainOrder = Notism.Domain.Order.Order;

namespace Notism.Application.Order.AdminUpdateOrderPaymentStatus;

public class AdminUpdateOrderPaymentStatusHandler : IRequestHandler<AdminUpdateOrderPaymentStatusRequest, AdminUpdateOrderPaymentStatusResponse>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IReadDbContext _readDbContext;
    private readonly ILogger<AdminUpdateOrderPaymentStatusHandler> _logger;
    private readonly IMessages _messages;

    public AdminUpdateOrderPaymentStatusHandler(
        IOrderRepository orderRepository,
        IReadDbContext readDbContext,
        ILogger<AdminUpdateOrderPaymentStatusHandler> logger,
        IMessages messages)
    {
        _orderRepository = orderRepository;
        _readDbContext = readDbContext;
        _logger = logger;
        _messages = messages;
    }

    public async Task<AdminUpdateOrderPaymentStatusResponse> Handle(
        AdminUpdateOrderPaymentStatusRequest request,
        CancellationToken cancellationToken)
    {
        var order = await _readDbContext.Set<DomainOrder>(tracking: true)
            .Where(o => o.Id == request.OrderId)
            .Include(o => o.User!)
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new ResultFailureException(_messages.OrderNotFound);

        var paymentStatus = request.PaymentStatus.ToEnum<PaymentStatus>();

        if (order.PaymentStatus == paymentStatus)
        {
            return AdminUpdateOrderPaymentStatusResponse.FromDomain(order);
        }

        order.UpdatePaymentStatus(paymentStatus);
        await _orderRepository.SaveChangesAsync();

        _logger.LogInformation(
            "Updated payment status of order {OrderId} to {Status}",
            request.OrderId,
            request.PaymentStatus);

        return AdminUpdateOrderPaymentStatusResponse.FromDomain(order);
    }
}