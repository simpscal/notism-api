using MediatR;

using Microsoft.Extensions.Logging;

using Notism.Application.Common.Interfaces;
using Notism.Domain.Cart;
using Notism.Domain.Common.Specifications;

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
        var specification = new FilterSpecification<CartItem>(c => c.UserId == userId)
            .Include(c => c.Food)
            .Include("Food.Category")
            .Include(c => c.Food.Images.OrderBy(i => i.DisplayOrder).Take(1))
            .Include("Food.CustomisationGroups.Options")
            .Include(c => c.Customisations);
        return (await _cartItemRepository.FilterByExpressionAsync(specification)).ToList();
    }
}
