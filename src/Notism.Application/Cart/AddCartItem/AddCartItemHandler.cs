using MediatR;

using Microsoft.Extensions.Logging;

using Notism.Domain.Cart;
using Notism.Domain.Common.Interfaces;
using Notism.Domain.Common.Specifications;
using Notism.Shared.Exceptions;

namespace Notism.Application.Cart.AddCartItem;

public class AddCartItemHandler : IRequestHandler<AddCartItemRequest, AddCartItemResponse>
{
    private readonly ICartItemRepository _cartItemRepository;
    private readonly IRepository<Domain.Food.Food> _foodRepository;
    private readonly ILogger<AddCartItemHandler> _logger;

    public AddCartItemHandler(
        ICartItemRepository cartItemRepository,
        IRepository<Domain.Food.Food> foodRepository,
        ILogger<AddCartItemHandler> logger)
    {
        _cartItemRepository = cartItemRepository;
        _foodRepository = foodRepository;
        _logger = logger;
    }

    public async Task<AddCartItemResponse> Handle(
        AddCartItemRequest request,
        CancellationToken cancellationToken)
    {
        // Check if food exists and is available
        var foodSpecification = new FilterSpecification<Domain.Food.Food>(f => f.Id == request.FoodId);
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

        // Check if cart item already exists
        var cartItemSpecification = new FilterSpecification<CartItem>(c => c.UserId == request.UserId && c.FoodId == request.FoodId)
            .Include(c => c.Food)
            .Include(c => c.Food.Images);
        var existingCartItem = await _cartItemRepository.FindByExpressionAsync(cartItemSpecification);

        // Update quantity if item already exists
        if (existingCartItem != null)
        {
            var newQuantity = existingCartItem.Quantity + request.Quantity;
            if (newQuantity > food.StockQuantity)
            {
                throw new ResultFailureException("Insufficient stock");
            }

            existingCartItem.UpdateQuantity(newQuantity);
            await _cartItemRepository.SaveChangesAsync();

            _logger.LogInformation(
                "Updated cart item {CartItemId} quantity to {Quantity} for user {UserId}",
                existingCartItem.Id,
                newQuantity,
                request.UserId);

            return new AddCartItemResponse { Id = existingCartItem.Id };
        }

        // Create new cart item
        var cartItem = CartItem.Create(request.UserId, request.FoodId, request.Quantity);
        _cartItemRepository.Add(cartItem);

        await _cartItemRepository.SaveChangesAsync();

        _logger.LogInformation(
            "Added cart item {CartItemId} for user {UserId}",
            cartItem.Id,
            request.UserId);

        return new AddCartItemResponse { Id = cartItem.Id };
    }
}