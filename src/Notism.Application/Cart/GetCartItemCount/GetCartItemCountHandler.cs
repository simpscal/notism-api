using MediatR;

using Microsoft.Extensions.Logging;

using Notism.Domain.Cart;
using Notism.Domain.Common.Specifications;

namespace Notism.Application.Cart.GetCartItemCount;

public class GetCartItemCountHandler : IRequestHandler<GetCartItemCountRequest, GetCartItemCountResponse>
{
    private readonly ICartItemRepository _cartItemRepository;
    private readonly ILogger<GetCartItemCountHandler> _logger;

    public GetCartItemCountHandler(
        ICartItemRepository cartItemRepository,
        ILogger<GetCartItemCountHandler> logger)
    {
        _cartItemRepository = cartItemRepository;
        _logger = logger;
    }

    public async Task<GetCartItemCountResponse> Handle(
        GetCartItemCountRequest request,
        CancellationToken cancellationToken)
    {
        var specification = new FilterSpecification<CartItem>(c => c.UserId == request.UserId);
        var cartItems = await _cartItemRepository.FilterByExpressionAsync(specification);

        var totalQuantity = cartItems.Sum(item => item.Quantity);
        var itemCount = cartItems.Count();

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