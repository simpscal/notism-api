using MediatR;

using Microsoft.Extensions.Logging;

using Notism.Application.Common.Services;
using Notism.Domain.Cart;
using Notism.Domain.Common.Specifications;
using Notism.Shared.Exceptions;

namespace Notism.Application.Cart.RemoveCartItem;

public class RemoveCartItemHandler : IRequestHandler<RemoveCartItemRequest>
{
    private readonly ICartItemRepository _cartItemRepository;
    private readonly ILogger<RemoveCartItemHandler> _logger;
    private readonly IMessages _messages;

    public RemoveCartItemHandler(
        ICartItemRepository cartItemRepository,
        ILogger<RemoveCartItemHandler> logger,
        IMessages messages)
    {
        _cartItemRepository = cartItemRepository;
        _logger = logger;
        _messages = messages;
    }

    public async Task Handle(
        RemoveCartItemRequest request,
        CancellationToken cancellationToken)
    {
        var specification = new FilterSpecification<CartItem>(c => c.Id == request.CartItemId);
        var cartItem = await _cartItemRepository.FindByExpressionAsync(specification)
            ?? throw new ResultFailureException(_messages.CartItemNotFound);

        if (cartItem.UserId != request.UserId)
        {
            throw new ResultFailureException(_messages.CartItemNotBelongToUser);
        }

        _cartItemRepository.Remove(cartItem);
        await _cartItemRepository.SaveChangesAsync();

        _logger.LogInformation(
            "Removed cart item {CartItemId} for user {UserId}",
            cartItem.Id,
            request.UserId);
    }
}