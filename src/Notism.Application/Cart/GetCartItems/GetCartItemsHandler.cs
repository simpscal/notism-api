using MediatR;

using Microsoft.Extensions.Logging;

using Notism.Application.Common.Interfaces;
using Notism.Domain.Cart;
using Notism.Domain.Common.Specifications;
using Notism.Shared.Extensions;

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
        var specification = new FilterSpecification<CartItem>(c => c.UserId == request.UserId)
            .Include(c => c.Food)
            .Include(c => c.Food.Images);
        var cartItems = await _cartItemRepository.FilterByExpressionAsync(specification);

        var items = cartItems.Select(cartItem => new CartItemResponse
        {
            Id = cartItem.Id,
            Name = cartItem.Food.Name,
            Description = cartItem.Food.Description,
            Price = cartItem.Food.Price,
            DiscountPrice = cartItem.Food.DiscountPrice,
            ImageUrl = GetImageUrl(cartItem.Food.Images),
            Category = cartItem.Food.Category.GetStringValue(),
            Quantity = cartItem.Quantity,
            StockQuantity = cartItem.Food.StockQuantity,
            QuantityUnit = cartItem.Food.QuantityUnit.GetStringValue(),
        }).ToList();

        _logger.LogInformation("Retrieved {Count} cart items for user {UserId}", items.Count, request.UserId);

        return new GetCartItemsResponse
        {
            Items = items,
        };
    }

    private string GetImageUrl(IReadOnlyCollection<Domain.Food.FoodImage> images)
    {
        var firstImage = images.OrderBy(img => img.DisplayOrder).FirstOrDefault();
        return firstImage == null ? string.Empty : _storageService.GetPublicUrl(firstImage.FileKey);
    }
}