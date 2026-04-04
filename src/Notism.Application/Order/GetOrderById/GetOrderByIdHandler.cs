using MediatR;

using Microsoft.Extensions.Logging;

using Notism.Application.Common.Interfaces;
using Notism.Application.Common.Services;
using Notism.Application.Order.Mappers;
using Notism.Application.Order.Models;
using Notism.Domain.Common.Specifications;
using Notism.Domain.Order;
using Notism.Domain.Order.Enums;
using Notism.Domain.Payment;
using Notism.Domain.Payment.Enums;
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
    private readonly IPaymentRepository _paymentRepository;

    public GetOrderByIdHandler(
        IOrderRepository orderRepository,
        IStorageService storageService,
        ILogger<GetOrderByIdHandler> logger,
        IMessages messages,
        IPaymentRepository paymentRepository)
    {
        _orderRepository = orderRepository;
        _storageService = storageService;
        _logger = logger;
        _messages = messages;
        _paymentRepository = paymentRepository;
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

        var paymentSpec = new FilterSpecification<Domain.Payment.Payment>(p => true);
        var payment = await _paymentRepository.FindByExpressionAsync(paymentSpec);
        var bankAccountConfigured = payment != null;

        PaymentQrResponse? paymentQr = null;
        if (bankAccountConfigured && order.PaymentMethod == PaymentMethod.Banking && order.PaymentStatus == PaymentStatus.Unpaid)
        {
            paymentQr = new PaymentQrResponse
            {
                BankCode = payment!.BankCode,
                AccountNumber = payment.AccountNumber,
                AccountHolderName = payment.AccountHolderName,
                Amount = order.TotalAmount,
                OrderReference = order.SlugId,
            };
        }

        var baseResponse = OrderMapper.ToResponse(order, _storageService, bankAccountConfigured);
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
            PaymentStatus = order.PaymentStatus.GetStringValue(),
            PaidAt = order.PaidAt,
            PaymentQr = paymentQr,
            BankAccountConfigured = bankAccountConfigured,
        };
    }
}