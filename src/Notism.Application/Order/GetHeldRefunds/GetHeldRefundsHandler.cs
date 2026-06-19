using MediatR;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using Notism.Application.Common.Persistence;
using Notism.Domain.Order.Enums;
using Notism.Domain.User.Enums;

using DomainBankAccount = Notism.Domain.User.BankAccount;
using DomainOrder = Notism.Domain.Order.Order;

namespace Notism.Application.Order.GetHeldRefunds;

public class GetHeldRefundsHandler : IRequestHandler<GetHeldRefundsRequest, GetHeldRefundsResponse>
{
    private readonly IReadDbContext _readDbContext;
    private readonly ILogger<GetHeldRefundsHandler> _logger;

    public GetHeldRefundsHandler(
        IReadDbContext readDbContext,
        ILogger<GetHeldRefundsHandler> logger)
    {
        _readDbContext = readDbContext;
        _logger = logger;
    }

    public async Task<GetHeldRefundsResponse> Handle(
        GetHeldRefundsRequest request,
        CancellationToken cancellationToken)
    {
        var hasBankDetails = await _readDbContext.Set<DomainBankAccount>()
            .AnyAsync(
                p => p.OwnerType == BankAccountOwnerType.Customer && p.OwnerId == request.UserId,
                cancellationToken);

        if (hasBankDetails)
        {
            _logger.LogInformation(
                "User {UserId} has bank details on file; no held refunds awaiting bank details",
                request.UserId);

            return GetHeldRefundsResponse.FromItems(Array.Empty<HeldRefundResponse>());
        }

        var orders = await _readDbContext.Set<DomainOrder>()
            .Where(o => o.UserId == request.UserId &&
                        o.Refund != null &&
                        (o.Refund.Status == RefundStatus.Pending ||
                         o.Refund.Status == RefundStatus.Processing))
            .Include(o => o.Refund)
            .ToListAsync(cancellationToken);

        var items = orders
            .Select(HeldRefundResponse.FromDomain)
            .ToList();

        _logger.LogInformation(
            "Retrieved {Count} held refunds awaiting bank details for user {UserId}",
            items.Count,
            request.UserId);

        return GetHeldRefundsResponse.FromItems(items);
    }
}
