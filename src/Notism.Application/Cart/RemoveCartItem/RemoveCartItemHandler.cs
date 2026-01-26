using MediatR;

using Microsoft.Extensions.Logging;

using Notism.Domain.Cart;
using Notism.Domain.Cart.Specifications;
using Notism.Shared.Exceptions;

namespace Notism.Application.Cart.RemoveCartItem;

public class RemoveCartItemHandler : IRequestHandler<RemoveCartItemRequest>
{
    private readonly ICartItemRepository _cartItemRepository;
    private readonly ILogger<RemoveCartItemHandler> _logger;

    public RemoveCartItemHandler(
        ICartItemRepository cartItemRepository,
        ILogger<RemoveCartItemHandler> logger)
    {
        _cartItemRepository = cartItemRepository;
        _logger = logger;
    }

    public async Task Handle(
        RemoveCartItemRequest request,
        CancellationToken cancellationToken)
    {
        var cartItem = await _cartItemRepository.FindByExpressionAsync(
            new CartItemByIdSpecification(request.CartItemId))
        ?? throw new ResultFailureException("Cart item not found");

        if (cartItem.UserId != request.UserId)
        {
            throw new ResultFailureException("Cart item does not belong to the user");
        }

        _cartItemRepository.Remove(cartItem);
        await _cartItemRepository.SaveChangesAsync();

        _logger.LogInformation(
            "Removed cart item {CartItemId} for user {UserId}",
            cartItem.Id,
            request.UserId);
    }
}

