using MediatR;

using Microsoft.Extensions.Logging;

using Notism.Application.Common.Interfaces;
using Notism.Application.Order.Common;
using Notism.Domain.Order;
using Notism.Domain.Order.Enums;
using Notism.Domain.Payment.Enums;
using Notism.Shared.Extensions;

namespace Notism.Application.Order.GetOrders;

public class GetOrdersHandler : IRequestHandler<GetOrdersRequest, GetOrdersResponse>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IStorageService _storageService;
    private readonly ILogger<GetOrdersHandler> _logger;

    public GetOrdersHandler(
        IOrderRepository orderRepository,
        IStorageService storageService,
        ILogger<GetOrdersHandler> logger)
    {
        _orderRepository = orderRepository;
        _storageService = storageService;
        _logger = logger;
    }

    public async Task<GetOrdersResponse> Handle(
        GetOrdersRequest request,
        CancellationToken cancellationToken)
    {
        var specification = new OrderDetailSpecification(
            o => o.UserId == request.UserId && o.DeliveryStatus != DeliveryStatus.Cancelled);
        var orders = await _orderRepository.FilterByExpressionAsync(specification);

        if (!string.IsNullOrWhiteSpace(request.PaymentStatus) &&
            request.PaymentStatus.ExistInEnum<PaymentStatus>())
        {
            var paymentStatusFilter = request.PaymentStatus.ToEnum<PaymentStatus>();
            orders = orders.Where(o => o.PaymentStatus == paymentStatusFilter).ToList();
        }

        var orderedOrders = orders
            .OrderByDescending(o => o.CreatedAt)
            .ToList();

        _logger.LogInformation("Retrieved {Count} orders for user {UserId}", orderedOrders.Count, request.UserId);

        return GetOrdersResponse.FromDomain(orderedOrders, _storageService);
    }
}