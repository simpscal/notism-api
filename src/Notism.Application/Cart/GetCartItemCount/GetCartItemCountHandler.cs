using MediatR;

using Microsoft.Extensions.Logging;

using Notism.Application.Common.Persistence;

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
        var quantities = await new GetCartItemCountQuery(_readDbContext).ExecuteAsync(request.UserId, cancellationToken);

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