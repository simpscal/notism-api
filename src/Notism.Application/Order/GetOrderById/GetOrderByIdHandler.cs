using MediatR;

using Microsoft.Extensions.Logging;

using Notism.Application.Common.Persistence;
using Notism.Application.Common.Services;
using Notism.Application.Order.Common;
using Notism.Domain.Order.Enums;
using Notism.Domain.Payment.Enums;
using Notism.Domain.User.Enums;
using Notism.Shared.Exceptions;
using Notism.Shared.Extensions;

namespace Notism.Application.Order.GetOrderById;

public class GetOrderByIdHandler : IRequestHandler<GetOrderByIdRequest, GetOrderByIdResponse>
{
    private readonly IReadDbContext _readDbContext;
    private readonly IStorageService _storageService;
    private readonly ILogger<GetOrderByIdHandler> _logger;
    private readonly IMessages _messages;

    public GetOrderByIdHandler(
        IReadDbContext readDbContext,
        IStorageService storageService,
        ILogger<GetOrderByIdHandler> logger,
        IMessages messages)
    {
        _readDbContext = readDbContext;
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

        var order = await new GetOrderByIdQuery(_readDbContext)
                .ExecuteAsync(request.SlugId, request.UserId, isAdmin, cancellationToken)
            ?? throw new ResultFailureException(_messages.OrderNotFound);

        _logger.LogInformation("Retrieved order {SlugId} for user {UserId} (Admin: {IsAdmin})", request.SlugId, request.UserId, isAdmin);

        var payment = await new GetBankAccountQuery(_readDbContext).ExecuteAsync(cancellationToken);
        var bankAccountConfigured = payment != null;

        PaymentQrResponse? paymentQr = null;
        if (bankAccountConfigured && order.PaymentMethod == PaymentMethod.Banking && order.PaymentStatus == PaymentStatus.Unpaid)
        {
            paymentQr = PaymentQrResponse.FromDomain(payment!, order.TotalAmount, order.SlugId);
        }

        return GetOrderByIdResponse.FromDomain(order, _storageService, paymentQr);
    }
}