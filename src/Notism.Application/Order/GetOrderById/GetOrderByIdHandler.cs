using MediatR;

using Microsoft.Extensions.Logging;

using Notism.Application.Common.Interfaces;
using Notism.Application.Order.Mappers;
using Notism.Domain.Common.Specifications;
using Notism.Domain.Order;
using Notism.Shared.Exceptions;

namespace Notism.Application.Order.GetOrderById;

public class GetOrderByIdHandler : IRequestHandler<GetOrderByIdRequest, GetOrderByIdResponse>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IStorageService _storageService;
    private readonly ILogger<GetOrderByIdHandler> _logger;

    public GetOrderByIdHandler(
        IOrderRepository orderRepository,
        IStorageService storageService,
        ILogger<GetOrderByIdHandler> logger)
    {
        _orderRepository = orderRepository;
        _storageService = storageService;
        _logger = logger;
    }

    public async Task<GetOrderByIdResponse> Handle(
        GetOrderByIdRequest request,
        CancellationToken cancellationToken)
    {
        var specification = new FilterSpecification<Domain.Order.Order>(o => o.SlugId == request.SlugId && o.UserId == request.UserId)
            .Include("Items.Food.Images")
            .Include(o => o.StatusHistory);
        var order = await _orderRepository.FindByExpressionAsync(specification)
            ?? throw new ResultFailureException("Order not found");

        _logger.LogInformation("Retrieved order {SlugId} for user {UserId}", request.SlugId, request.UserId);

        var baseResponse = OrderMapper.ToResponse(order, _storageService);
        return new GetOrderByIdResponse
        {
            Id = baseResponse.Id,
            SlugId = baseResponse.SlugId,
            TotalAmount = baseResponse.TotalAmount,
            PaymentMethod = baseResponse.PaymentMethod,
            DeliveryStatus = baseResponse.DeliveryStatus,
            CreatedAt = baseResponse.CreatedAt,
            UpdatedAt = baseResponse.UpdatedAt,
            Items = baseResponse.Items,
            DeliveryStatusTiming = baseResponse.DeliveryStatusTiming,
        };
    }
}