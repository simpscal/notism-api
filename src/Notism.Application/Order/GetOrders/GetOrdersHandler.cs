using System.Linq.Expressions;

using MediatR;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using Notism.Application.Common.Persistence;
using Notism.Application.Common.Services;
using Notism.Domain.Order.Enums;
using Notism.Domain.Payment.Enums;
using Notism.Shared.Extensions;
using Notism.Shared.Models;

using DomainOrder = Notism.Domain.Order.Order;

namespace Notism.Application.Order.GetOrders;

public class GetOrdersHandler : IRequestHandler<GetOrdersRequest, GetOrdersResponse>
{
    private readonly IReadDbContext _readDbContext;
    private readonly IStorageService _storageService;
    private readonly ILogger<GetOrdersHandler> _logger;

    public GetOrdersHandler(
        IReadDbContext readDbContext,
        IStorageService storageService,
        ILogger<GetOrdersHandler> logger)
    {
        _readDbContext = readDbContext;
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

        var totalCount = await _readDbContext.BuildGraphQuery<DomainOrder>(
                filter,
                includes => includes
                    .Include("Items.Food.Images")
                    .Include(o => o.StatusHistory),
                query => query
                    .OrderByDescending(o => o.CreatedAt)
                    .ThenByDescending(o => o.Id))
            .CountAsync(cancellationToken);

        var orders = await _readDbContext.BuildGraphQuery<DomainOrder>(
                filter,
                request.Skip,
                request.Take,
                includes => includes
                    .Include("Items.Food.Images")
                    .Include(o => o.StatusHistory),
                query => query
                    .OrderByDescending(o => o.CreatedAt)
                    .ThenByDescending(o => o.Id))
            .ToListAsync(cancellationToken);

        _logger.LogInformation(
            "Retrieved {Count} of {TotalCount} orders for user {UserId}",
            orders.Count,
            totalCount,
            request.UserId);

        var pagedResult = new PagedResult<DomainOrder>
        {
            TotalCount = totalCount,
            Items = orders,
        };

        return GetOrdersResponse.FromDomain(pagedResult, _storageService);
    }
}