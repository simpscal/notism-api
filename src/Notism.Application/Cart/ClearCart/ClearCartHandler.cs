using MediatR;

using Microsoft.Extensions.Logging;

using Notism.Domain.Cart;

namespace Notism.Application.Cart.ClearCart;

public class ClearCartHandler : IRequestHandler<ClearCartRequest>
{
    private readonly ICartItemRepository _cartItemRepository;
    private readonly ILogger<ClearCartHandler> _logger;

    public ClearCartHandler(
        ICartItemRepository cartItemRepository,
        ILogger<ClearCartHandler> logger)
    {
        _cartItemRepository = cartItemRepository;
        _logger = logger;
    }

    public async Task Handle(
        ClearCartRequest request,
        CancellationToken cancellationToken)
    {
        await _cartItemRepository.ClearCart(request.UserId);
        await _cartItemRepository.SaveChangesAsync();

        _logger.LogInformation("Cleared cart for user {UserId}", request.UserId);
    }
}