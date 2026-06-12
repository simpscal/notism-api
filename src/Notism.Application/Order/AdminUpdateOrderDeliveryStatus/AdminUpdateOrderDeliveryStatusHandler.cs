using MediatR;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using Notism.Application.Common.Persistence;
using Notism.Application.Common.Services;
using Notism.Domain.Order.Enums;
using Notism.Domain.Order.Repositories;
using Notism.Shared.Exceptions;
using Notism.Shared.Extensions;

using DomainOrder = Notism.Domain.Order.Order;

namespace Notism.Application.Order.AdminUpdateOrderDeliveryStatus;

public class AdminUpdateOrderDeliveryStatusHandler : IRequestHandler<AdminUpdateOrderDeliveryStatusRequest, AdminUpdateOrderDeliveryStatusResponse>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IReadDbContext _readDbContext;
    private readonly ILogger<AdminUpdateOrderDeliveryStatusHandler> _logger;
    private readonly IMessages _messages;

    public AdminUpdateOrderDeliveryStatusHandler(
        IOrderRepository orderRepository,
        IReadDbContext readDbContext,
        ILogger<AdminUpdateOrderDeliveryStatusHandler> logger,
        IMessages messages)
    {
        _orderRepository = orderRepository;
        _readDbContext = readDbContext;
        _logger = logger;
        _messages = messages;
    }

    public async Task<AdminUpdateOrderDeliveryStatusResponse> Handle(
        AdminUpdateOrderDeliveryStatusRequest request,
        CancellationToken cancellationToken)
    {
        var order = await _readDbContext.Set<DomainOrder>(tracking: true)
            .Where(o => o.Id == request.OrderId)
            .Include(o => o.User!)
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new ResultFailureException(_messages.OrderNotFound);

        var deliveryStatus = request.DeliveryStatus.ToEnum<DeliveryStatus>();

        order.UpdateDeliveryStatus(deliveryStatus);
        await _orderRepository.SaveChangesAsync();

        _logger.LogInformation(
            "Updated delivery status of order {OrderId} to {Status}",
            request.OrderId,
            request.DeliveryStatus);

        return AdminUpdateOrderDeliveryStatusResponse.FromDomain(order);
    }
}