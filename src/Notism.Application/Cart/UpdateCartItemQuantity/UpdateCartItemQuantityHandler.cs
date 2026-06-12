using MediatR;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using Notism.Application.Common.Persistence;
using Notism.Application.Common.Services;
using Notism.Domain.Cart;
using Notism.Domain.Cart.Repositories;
using Notism.Shared.Exceptions;

namespace Notism.Application.Cart.UpdateCartItemQuantity;

public class UpdateCartItemQuantityHandler : IRequestHandler<UpdateCartItemQuantityRequest, UpdateCartItemQuantityResponse>
{
    private readonly ICartItemRepository _cartItemRepository;
    private readonly IReadDbContext _readDbContext;
    private readonly ILogger<UpdateCartItemQuantityHandler> _logger;
    private readonly IMessages _messages;

    public UpdateCartItemQuantityHandler(
        ICartItemRepository cartItemRepository,
        IReadDbContext readDbContext,
        ILogger<UpdateCartItemQuantityHandler> logger,
        IMessages messages)
    {
        _cartItemRepository = cartItemRepository;
        _readDbContext = readDbContext;
        _logger = logger;
        _messages = messages;
    }

    public async Task<UpdateCartItemQuantityResponse> Handle(
        UpdateCartItemQuantityRequest request,
        CancellationToken cancellationToken)
    {
        // Loaded TRACKED so the quantity mutation persists on SaveChanges via the same context.
        var cartItem = await _readDbContext.Set<CartItem>(tracking: true)
                .Where(c => c.Id == request.CartItemId)
                .FirstOrDefaultAsync(cancellationToken)
            ?? throw new ResultFailureException(_messages.CartItemNotFound);

        // Verify the cart item belongs to the user
        if (cartItem.UserId != request.UserId)
        {
            throw new ResultFailureException(_messages.CartItemNotBelongToUser);
        }

        // Check if food is still available (read-only lookup)
        var food = await _readDbContext.Set<Domain.Food.Food>()
                .Where(f => f.Id == cartItem.FoodId)
                .FirstOrDefaultAsync(cancellationToken)
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