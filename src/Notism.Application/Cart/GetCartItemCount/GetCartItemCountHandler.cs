using MediatR;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using Notism.Application.Common.Persistence;
using Notism.Domain.Cart;

namespace Notism.Application.Cart.GetCartItemCount;

public class GetCartItemCountHandler : IRequestHandler<GetCartItemCountRequest, GetCartItemCountResponse>
{
    private readonly IReadDbContext _readDbContext;
    private readonly ILogger<GetCartItemCountHandler> _logger;

    public GetCartItemCountHandler(
        IReadDbContext readDbContext,
        ILogger<GetCartItemCountHandler> logger)
    {
        _readDbContext = readDbContext;
        _logger = logger;
    }

    public async Task<GetCartItemCountResponse> Handle(
        GetCartItemCountRequest request,
        CancellationToken cancellationToken)
    {
        var quantities = await _readDbContext.Set<CartItem>()
            .Where(c => c.UserId == request.UserId)
            .Select(c => c.Quantity)
            .ToListAsync(cancellationToken);

        var totalQuantity = quantities.Sum();
        var itemCount = quantities.Count;

        _logger.LogInformation(
            "Retrieved cart count for user {UserId}: {ItemCount} items with total quantity {TotalQuantity}",
            request.UserId,
            itemCount,
            totalQuantity);

        return new GetCartItemCountResponse
        {
            TotalQuantity = totalQuantity,
            ItemCount = itemCount,
        };
    }
}