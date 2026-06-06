using MediatR;

using Microsoft.Extensions.Logging;

using Notism.Application.Common.Interfaces;
using Notism.Application.Order.Common;
using Notism.Domain.Common.Specifications;
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
        var specification = new FilterSpecification<Domain.Order.Order>(
                o => o.UserId == request.UserId && o.DeliveryStatus != DeliveryStatus.Cancelled)
            .Include("Items.Food.Images")
            .Include(o => o.StatusHistory);
        var orders = await _orderRepository.FilterByExpressionAsync(specification);

        if (!string.IsNullOrWhiteSpace(request.PaymentStatus) &&
            request.PaymentStatus.ExistInEnum<PaymentStatus>())
        {
            var paymentStatusFilter = request.PaymentStatus.ToEnum<PaymentStatus>();
            orders = orders.Where(o => o.PaymentStatus == paymentStatusFilter).ToList();
        }

        var orderResponses = orders
            .OrderByDescending(o => o.CreatedAt)
            .Select(MapToResponse)
            .ToList();

        _logger.LogInformation("Retrieved {Count} orders for user {UserId}", orderResponses.Count, request.UserId);

        return new GetOrdersResponse
        {
            Orders = orderResponses,
        };
    }

    private GetOrdersOrderResponse MapToResponse(Domain.Order.Order order)
    {
        return new GetOrdersOrderResponse
        {
            Id = order.Id,
            SlugId = order.SlugId,
            TotalAmount = order.TotalAmount,
            PaymentMethod = order.PaymentMethod.GetStringValue(),
            DeliveryStatus = order.DeliveryStatus.GetStringValue(),
            CreatedAt = order.CreatedAt,
            UpdatedAt = order.UpdatedAt,
            Items = order.Items.Select(item => OrderItemResponse.FromDomain(item, _storageService)).ToList(),
            DeliveryStatusTiming = DeliveryStatusTimingResponse.FromDomain(order.GetDeliveryStatusTiming()),
            PaymentStatus = order.PaymentStatus.GetStringValue(),
            PaidAt = order.PaidAt,
            DeliveryNotes = order.DeliveryNotes,
        };
    }
}