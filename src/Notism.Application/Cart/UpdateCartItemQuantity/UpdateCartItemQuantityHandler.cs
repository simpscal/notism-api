using MediatR;

using Microsoft.Extensions.Logging;

using Notism.Domain.Cart;
using Notism.Domain.Common.Interfaces;
using Notism.Domain.Common.Specifications;
using Notism.Shared.Exceptions;

namespace Notism.Application.Cart.UpdateCartItemQuantity;

public class UpdateCartItemQuantityHandler : IRequestHandler<UpdateCartItemQuantityRequest, UpdateCartItemQuantityResponse>
{
    private readonly ICartItemRepository _cartItemRepository;
    private readonly IRepository<Domain.Food.Food> _foodRepository;
    private readonly ILogger<UpdateCartItemQuantityHandler> _logger;

    public UpdateCartItemQuantityHandler(
        ICartItemRepository cartItemRepository,
        IRepository<Domain.Food.Food> foodRepository,
        ILogger<UpdateCartItemQuantityHandler> logger)
    {
        _cartItemRepository = cartItemRepository;
        _foodRepository = foodRepository;
        _logger = logger;
    }

    public async Task<UpdateCartItemQuantityResponse> Handle(
        UpdateCartItemQuantityRequest request,
        CancellationToken cancellationToken)
    {
        var cartItemSpecification = new FilterSpecification<CartItem>(c => c.Id == request.CartItemId);
        var cartItem = await _cartItemRepository.FindByExpressionAsync(cartItemSpecification)
            ?? throw new ResultFailureException("Cart item not found");

        // Verify the cart item belongs to the user
        if (cartItem.UserId != request.UserId)
        {
            throw new ResultFailureException("Cart item does not belong to the user");
        }

        // Check if food is still available
        var foodSpecification = new FilterSpecification<Domain.Food.Food>(f => f.Id == cartItem.FoodId);
        var food = await _foodRepository.FindByExpressionAsync(foodSpecification)
            ?? throw new ResultFailureException("Food not found");

        if (!food.IsAvailable)
        {
            throw new ResultFailureException("Food is not available");
        }

        if (request.Quantity > food.StockQuantity)
        {
            throw new ResultFailureException("Insufficient stock");
        }

        cartItem.UpdateQuantity(request.Quantity);
        await _cartItemRepository.SaveChangesAsync();

        _logger.LogInformation(
            "Updated cart item {CartItemId} quantity to {Quantity} for user {UserId}",
            cartItem.Id,
            request.Quantity,
            request.UserId);

        return new UpdateCartItemQuantityResponse { Id = cartItem.Id };
    }
}