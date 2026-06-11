using MediatR;

using Microsoft.Extensions.Logging;

using Notism.Application.Common.Services;
using Notism.Domain.Cart.Repositories;
using Notism.Domain.Common.Repositories;
using Notism.Shared.Exceptions;

namespace Notism.Application.Cart.UpdateCartItemQuantity;

public class UpdateCartItemQuantityHandler : IRequestHandler<UpdateCartItemQuantityRequest, UpdateCartItemQuantityResponse>
{
    private readonly ICartItemRepository _cartItemRepository;
    private readonly IRepository<Domain.Food.Food> _foodRepository;
    private readonly ILogger<UpdateCartItemQuantityHandler> _logger;
    private readonly IMessages _messages;

    public UpdateCartItemQuantityHandler(
        ICartItemRepository cartItemRepository,
        IRepository<Domain.Food.Food> foodRepository,
        ILogger<UpdateCartItemQuantityHandler> logger,
        IMessages messages)
    {
        _cartItemRepository = cartItemRepository;
        _foodRepository = foodRepository;
        _logger = logger;
        _messages = messages;
    }

    public async Task<UpdateCartItemQuantityResponse> Handle(
        UpdateCartItemQuantityRequest request,
        CancellationToken cancellationToken)
    {
        var cartItem = await _cartItemRepository.GetForUpdateAsync(c => c.Id == request.CartItemId)
            ?? throw new ResultFailureException(_messages.CartItemNotFound);

        // Verify the cart item belongs to the user
        if (cartItem.UserId != request.UserId)
        {
            throw new ResultFailureException(_messages.CartItemNotBelongToUser);
        }

        // Check if food is still available
        var food = await _foodRepository.GetForUpdateAsync(f => f.Id == cartItem.FoodId)
            ?? throw new ResultFailureException(_messages.FoodNotFound);

        if (!food.IsAvailable)
        {
            throw new ResultFailureException(_messages.FoodNotAvailable);
        }

        if (request.Quantity > food.StockQuantity)
        {
            throw new ResultFailureException(_messages.InsufficientStock);
        }

        cartItem.UpdateQuantity(request.Quantity);
        await _cartItemRepository.SaveChangesAsync();

        _logger.LogInformation(
            "Updated cart item {CartItemId} quantity to {Quantity} for user {UserId}",
            cartItem.Id,
            request.Quantity,
            request.UserId);

        return UpdateCartItemQuantityResponse.FromDomain(cartItem);
    }
}