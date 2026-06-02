using MediatR;

using Microsoft.Extensions.Logging;

using Notism.Application.Cart.Models;
using Notism.Application.Common.Constants;
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
            .Include(c => c.Food.Images.OrderBy(i => i.DisplayOrder).Take(1))
            .Include("Food.CustomisationGroups.Options");
        return await _cartItemRepository.FilterByExpressionAsync(specification);
    }

    private List<CartItemResponse> MapToResponse(IEnumerable<CartItem> cartItems)
    {
        return cartItems.Select(cartItem =>
        {
            var response = new CartItemResponse
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
            };

            if (cartItem.CustomisationGroupId.HasValue)
            {
                var group = cartItem.Food.CustomisationGroups
                    .FirstOrDefault(g => g.Id == cartItem.CustomisationGroupId.Value);

                if (group != null)
                {
                    response = response with
                    {
                        CustomisationGroupId = group.Id,
                        CustomisationGroupLabel = group.Label,
                        CustomisationOptions = group.Options
                            .Select(o => new CustomisationOptionResponse
                            {
                                Id = o.Id,
                                Label = o.Label,
                                Surcharge = o.Surcharge,
                            })
                            .ToList(),
                    };

                    // Verify the stored option still exists in the group
                    var optionStillExists = group.Options.Any(o => o.Id == cartItem.CustomisationOptionId);

                    if (optionStillExists && cartItem.CustomisationOptionId.HasValue)
                    {
                        response = response with
                        {
                            CustomisationOptionId = cartItem.CustomisationOptionId,
                            CustomisationLabel = cartItem.CustomisationLabel,
                            Surcharge = cartItem.Surcharge,
                        };
                    }
                    else
                    {
                        // Orphaned option — the option no longer exists in the food's group
                        response = response with
                        {
                            CustomisationOptionId = null,
                            CustomisationLabel = "Option no longer available",
                            Surcharge = null,
                        };
                    }
                }
            }

            return response;
        }).ToList();
    }

    private string GetImageUrl(IReadOnlyCollection<Domain.Food.FoodImage> images)
    {
        var firstImage = images.OrderBy(img => img.DisplayOrder).FirstOrDefault();
        return firstImage == null ? string.Empty : _storageService.GetPublicUrl(firstImage.FileKey, StorageTypeConstants.Food);
    }
}
