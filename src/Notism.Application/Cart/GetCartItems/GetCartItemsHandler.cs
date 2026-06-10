using MediatR;

using Microsoft.Extensions.Logging;

using Notism.Application.Cart.Common;
using Notism.Application.Common.Services;
using Notism.Domain.Cart;
using Notism.Domain.Cart.Repositories;

namespace Notism.Application.Cart.GetCartItems;

public class GetCartItemsHandler : IRequestHandler<GetCartItemsRequest, GetCartItemsResponse>
{
    private readonly ICartItemRepository _cartItemRepository;
    private readonly IStorageService _storageService;
    private readonly ILogger<GetCartItemsHandler> _logger;

    public GetCartItemsHandler(
        ICartItemRepository cartItemRepository,
        IStorageService storageService,
        ILogger<GetCartItemsHandler> logger)
    {
        _cartItemRepository = cartItemRepository;
        _storageService = storageService;
        _logger = logger;
    }

    public async Task<GetCartItemsResponse> Handle(
        GetCartItemsRequest request,
        CancellationToken cancellationToken)
    {
        var cartItems = await FetchCartItemsAsync(request.UserId);

        _logger.LogInformation("Retrieved {Count} cart items for user {UserId}", cartItems.Count, request.UserId);

        return GetCartItemsResponse.FromDomain(cartItems, _storageService);
    }

    private async Task<List<CartItem>> FetchCartItemsAsync(Guid userId)
    {
        var specification = new CartItemDetailSpecification(c => c.UserId == userId);
        return (await _cartItemRepository.FilterByExpressionAsync(specification)).ToList();
    }
}
