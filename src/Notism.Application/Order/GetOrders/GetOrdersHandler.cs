using System.Linq.Expressions;

using MediatR;

using Microsoft.Extensions.Logging;

using Notism.Application.Common.Interfaces;
using Notism.Application.Order.Common;
using Notism.Domain.Order;
using Notism.Domain.Order.Enums;
using Notism.Domain.Payment.Enums;
using Notism.Shared.Extensions;

using DomainOrder = Notism.Domain.Order.Order;

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
        Expression<Func<DomainOrder, bool>> filter =
            o => o.UserId == request.UserId && o.DeliveryStatus != DeliveryStatus.Cancelled;

        if (!string.IsNullOrWhiteSpace(request.PaymentStatus) &&
            request.PaymentStatus.ExistInEnum<PaymentStatus>())
        {
            var paymentStatus = request.PaymentStatus.ToEnum<PaymentStatus>();
            filter = o => o.UserId == request.UserId &&
                          o.DeliveryStatus != DeliveryStatus.Cancelled &&
                          o.PaymentStatus == paymentStatus;
        }

        var specification = new OrderDetailSpecification(filter);

        var pagedResult = await _orderRepository.FilterPagedByExpressionAsync(specification, request);

        _logger.LogInformation(
            "Retrieved {Count} of {TotalCount} orders for user {UserId}",
            pagedResult.Items.Count(),
            pagedResult.TotalCount,
            request.UserId);

        return GetOrdersResponse.FromDomain(pagedResult, _storageService);
    }
}