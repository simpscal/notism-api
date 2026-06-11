using MediatR;

using Microsoft.Extensions.Logging;

using Notism.Application.Common.Persistence;
using Notism.Domain.Payment.Enums;
using Notism.Shared.Extensions;

namespace Notism.Application.Order.AdminOrdersForTable;

public class AdminOrdersForTableHandler : IRequestHandler<AdminOrdersForTableRequest, AdminOrdersForTableResponse>
{
    private readonly IReadDbContext _readDbContext;
    private readonly ILogger<AdminOrdersForTableHandler> _logger;

    public AdminOrdersForTableHandler(
        IReadDbContext readDbContext,
        ILogger<AdminOrdersForTableHandler> logger)
    {
        _readDbContext = readDbContext;
        _logger = logger;
    }

    public async Task<AdminOrdersForTableResponse> Handle(
        AdminOrdersForTableRequest request,
        CancellationToken cancellationToken)
    {
        PaymentStatus? paymentStatus = null;
        if (!string.IsNullOrWhiteSpace(request.PaymentStatus) &&
            request.PaymentStatus.ExistInEnum<PaymentStatus>())
        {
            paymentStatus = request.PaymentStatus.ToEnum<PaymentStatus>();
        }

        var (totalCount, orders) = await new AdminOrdersForTableQuery(_readDbContext).ExecuteAsync(
            request.Keyword,
            request.SortBy,
            request.SortOrder,
            paymentStatus,
            request.Skip,
            request.Take,
            cancellationToken);

        var items = orders.Select(AdminOrdersForTableOrderResponse.FromDomain).ToList();

        _logger.LogInformation("Retrieved {Count} orders for table view", items.Count);

        return new AdminOrdersForTableResponse
        {
            TotalCount = totalCount,
            Items = items,
        };
    }
}