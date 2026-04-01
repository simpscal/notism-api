using MediatR;

using Microsoft.Extensions.Logging;

using Notism.Application.Common.Interfaces;
using Notism.Application.Common.Services;
using Notism.Application.Order.Mappers;
using Notism.Domain.Common.Specifications;
using Notism.Domain.Order;
using Notism.Domain.User.Enums;
using Notism.Shared.Exceptions;
using Notism.Shared.Extensions;

namespace Notism.Application.Order.GetOrderById;

public class GetOrderByIdHandler : IRequestHandler<GetOrderByIdRequest, GetOrderByIdResponse>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IStorageService _storageService;
    private readonly ILogger<GetOrderByIdHandler> _logger;
    private readonly IMessages _messages;

    public GetOrderByIdHandler(
        IOrderRepository orderRepository,
        IStorageService storageService,
        ILogger<GetOrderByIdHandler> logger,
        IMessages messages)
    {
        _orderRepository = orderRepository;
        _storageService = storageService;
        _logger = logger;
        _messages = messages;
    }

    public async Task<GetOrderByIdResponse> Handle(
        GetOrderByIdRequest request,
        CancellationToken cancellationToken)
    {
        var userRole = request.Role.FromCamelCase<UserRole>() ?? UserRole.User;
        var isAdmin = userRole == UserRole.Admin;

        var specification = new FilterSpecification<Domain.Order.Order>(o =>
            o.SlugId == request.SlugId &&
            (o.UserId == request.UserId || isAdmin))
            .Include("Items.Food.Images")
            .Include(o => o.StatusHistory);
        var order = await _orderRepository.FindByExpressionAsync(specification)
            ?? throw new ResultFailureException(_messages.OrderNotFound);

        _logger.LogInformation("Retrieved order {SlugId} for user {UserId} (Admin: {IsAdmin})", request.SlugId, request.UserId, isAdmin);

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