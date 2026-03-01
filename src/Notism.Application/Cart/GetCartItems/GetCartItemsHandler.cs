using MediatR;

using Microsoft.Extensions.Logging;

using Notism.Application.Cart.Models;
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
    private GetCartItemsRequest? _request;

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
        _request = request;

        var cartItems = await FetchCartItemsAsync();
        var items = MapToResponse(cartItems);

        _logger.LogInformation("Retrieved {Count} cart items for user {UserId}", items.Count, _request.UserId);

        return new GetCartItemsResponse
        {
            Items = items,
        };
    }

    private async Task<IEnumerable<CartItem>> FetchCartItemsAsync()
    {
        var specification = new FilterSpecification<CartItem>(c => c.UserId == _request!.UserId)
            .Include(c => c.Food)
            .Include("Food.Category")
            .Include(c => c.Food.Images.OrderBy(i => i.DisplayOrder).Take(1));
        return await _cartItemRepository.FilterByExpressionAsync(specification);
    }

    private List<CartItemResponse> MapToResponse(IEnumerable<CartItem> cartItems)
    {
        return cartItems.Select(cartItem => new CartItemResponse
        {
            Id = cartItem.Id,
            FoodId = cartItem.FoodId,
            Name = cartItem.Food.Name,
            Description = cartItem.Food.Description,
            Price = cartItem.Food.Price,
            DiscountPrice = cartItem.Food.DiscountPrice,
            ImageUrl = GetImageUrl(cartItem.Food.Images),
            Category = cartItem.Food.Category?.Name ?? string.Empty,
            Quantity = cartItem.Quantity,
            StockQuantity = cartItem.Food.StockQuantity,
            QuantityUnit = cartItem.Food.QuantityUnit.GetStringValue(),
        }).ToList();
    }

    private string GetImageUrl(IReadOnlyCollection<Domain.Food.FoodImage> images)
    {
        var firstImage = images.OrderBy(img => img.DisplayOrder).FirstOrDefault();
        return firstImage == null ? string.Empty : _storageService.GetPublicUrl(firstImage.FileKey);
    }
}