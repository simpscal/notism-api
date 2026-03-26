using MediatR;

using Microsoft.Extensions.Logging;

using Notism.Application.Common.Services;
using Notism.Domain.Cart;
using Notism.Domain.Common.Interfaces;
using Notism.Domain.Common.Specifications;
using Notism.Domain.Order;
using Notism.Domain.Order.Enums;
using Notism.Shared.Exceptions;
using Notism.Shared.Extensions;

namespace Notism.Application.Order.CreateOrder;

public class CreateOrderHandler : IRequestHandler<CreateOrderRequest, CreateOrderResponse>
{
    private readonly ICartItemRepository _cartItemRepository;
    private readonly IOrderRepository _orderRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CreateOrderHandler> _logger;
    private readonly IMessages _messages;

    public CreateOrderHandler(
        ICartItemRepository cartItemRepository,
        IOrderRepository orderRepository,
        IUnitOfWork unitOfWork,
        ILogger<CreateOrderHandler> logger,
        IMessages messages)
    {
        _cartItemRepository = cartItemRepository;
        _orderRepository = orderRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
        _messages = messages;
    }

    public async Task<CreateOrderResponse> Handle(
        CreateOrderRequest request,
        CancellationToken cancellationToken)
    {
        return await _unitOfWork.ExecuteInTransactionAsync(async () =>
        {
            var cartItems = await ValidateAndFetchCartItemsAsync(request);
            var paymentMethod = request.PaymentMethod.ToEnum<PaymentMethod>();
            var order = Domain.Order.Order.Create(request.UserId, paymentMethod, request.CartItemIds);

            AddOrderItems(order, cartItems);
            RemoveCartItems(cartItems);

            _logger.LogInformation(
                "Created order {OrderId} for user {UserId} with {ItemCount} items and total amount {TotalAmount}",
                order.Id,
                request.UserId,
                cartItems.Count(),
                order.TotalAmount);

            return new CreateOrderResponse
            {
                OrderId = order.Id,
                SlugId = order.SlugId,
                TotalAmount = order.TotalAmount,
                PaymentMethod = order.PaymentMethod.GetStringValue(),
                DeliveryStatus = order.DeliveryStatus.GetStringValue(),
                CreatedAt = order.CreatedAt,
            };
        });
    }

    private async Task<List<CartItem>> ValidateAndFetchCartItemsAsync(CreateOrderRequest request)
    {
        var cartItemSpecification = new FilterSpecification<CartItem>(c => c.UserId == request.UserId && request.CartItemIds.Contains(c.Id))
            .Include(c => c.Food)
            .Include(c => c.Food.Images);
        var cartItems = (await _cartItemRepository.FilterByExpressionAsync(cartItemSpecification)).ToList();

        if (cartItems.Count == 0)
        {
            throw new ResultFailureException(_messages.NoCartItemsFound);
        }

        var foundIds = cartItems.Select(c => c.Id).ToHashSet();
        var missingIds = request.CartItemIds.Where(id => !foundIds.Contains(id)).ToList();

        return missingIds.Any() ? throw new ResultFailureException(string.Format(_messages.CartItemsNotFound, string.Join(", ", missingIds))) : cartItems;
    }

    private void AddOrderItems(Domain.Order.Order order, List<CartItem> cartItems)
    {
        foreach (var cartItem in cartItems)
        {
            var orderItem = OrderItem.Create(
                order.Id,
                cartItem.FoodId,
                cartItem.Food.Name,
                cartItem.Food.Price,
                cartItem.Food.DiscountPrice,
                cartItem.Quantity);

            order.AddItem(orderItem);
            cartItem.Food.DeductStock(cartItem.Quantity);
        }

        _orderRepository.Add(order);
    }

    private void RemoveCartItems(List<CartItem> cartItems)
    {
        foreach (var cartItem in cartItems)
        {
            _cartItemRepository.Remove(cartItem);
        }
    }
}