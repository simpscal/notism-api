using MediatR;

using Microsoft.Extensions.Logging;

using Notism.Application.Cart.Models;
using Notism.Application.Common.Interfaces;
using Notism.Domain.Cart;
using Notism.Domain.Common.Interfaces;
using Notism.Domain.Common.Specifications;
using Notism.Shared.Exceptions;
using Notism.Shared.Extensions;

namespace Notism.Application.Cart.AddBulkCartItems;

public class AddBulkCartItemsHandler : IRequestHandler<AddBulkCartItemsRequest, AddBulkCartItemsResponse>
{
    private readonly ICartItemRepository _cartItemRepository;
    private readonly IRepository<Domain.Food.Food> _foodRepository;
    private readonly IStorageService _storageService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<AddBulkCartItemsHandler> _logger;
    private AddBulkCartItemsRequest? _request;

    public AddBulkCartItemsHandler(
        ICartItemRepository cartItemRepository,
        IRepository<Domain.Food.Food> foodRepository,
        IStorageService storageService,
        IUnitOfWork unitOfWork,
        ILogger<AddBulkCartItemsHandler> logger)
    {
        _cartItemRepository = cartItemRepository;
        _foodRepository = foodRepository;
        _storageService = storageService;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<AddBulkCartItemsResponse> Handle(
        AddBulkCartItemsRequest request,
        CancellationToken cancellationToken)
    {
        _request = request;

        return await _unitOfWork.ExecuteInTransactionAsync(async () =>
        {
            await ClearUserCartAsync();

            var foodDictionary = await FetchFoodsAsync();
            var addedCartItems = AddCartItems(foodDictionary);
            var items = MapToResponse(addedCartItems);

            _logger.LogInformation(
                "Bulk add cart items completed for user {UserId}: {Count} items added",
                _request.UserId,
                items.Count);

            return new AddBulkCartItemsResponse
            {
                Items = items,
            };
        });
    }

    private async Task ClearUserCartAsync()
    {
        await _cartItemRepository.ClearCart(_request!.UserId);
        _logger.LogInformation("Cleared existing cart items for user {UserId}", _request.UserId);
    }

    private async Task<Dictionary<Guid, Domain.Food.Food>> FetchFoodsAsync()
    {
        var foodIds = _request!.Items.Select(i => i.FoodId).Distinct().ToList();
        var foodSpecification = new FilterSpecification<Domain.Food.Food>(f => foodIds.Contains(f.Id))
            .Include(f => f.Images.OrderBy(i => i.DisplayOrder).Take(1));
        var foods = await _foodRepository.FilterByExpressionAsync(foodSpecification);

        return foods.ToDictionary(f => f.Id);
    }

    private List<(CartItem CartItem, Domain.Food.Food Food)> AddCartItems(
        Dictionary<Guid, Domain.Food.Food> foodDictionary)
    {
        var addedCartItems = new List<(CartItem CartItem, Domain.Food.Food Food)>();

        foreach (var item in _request!.Items)
        {
            try
            {
                var cartItem = ValidateAndAddCartItem(item, foodDictionary);
                addedCartItems.Add(cartItem);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(
                    ex,
                    "Failed to add cart item for FoodId {FoodId} and user {UserId}",
                    item.FoodId,
                    _request.UserId);
            }
        }

        return addedCartItems;
    }

    private (CartItem CartItem, Domain.Food.Food Food) ValidateAndAddCartItem(
        CartItemRequest item,
        Dictionary<Guid, Domain.Food.Food> foodDictionary)
    {
        if (!foodDictionary.TryGetValue(item.FoodId, out var food))
        {
            throw new ResultFailureException("Food not found");
        }

        if (!food.IsAvailable)
        {
            throw new ResultFailureException("Food is not available");
        }

        if (item.Quantity > food.StockQuantity)
        {
            throw new ResultFailureException("Insufficient stock");
        }

        var cartItem = CartItem.Create(_request!.UserId, item.FoodId, item.Quantity);
        _cartItemRepository.Add(cartItem);

        return (cartItem, food);
    }

    private List<CartItemResponse> MapToResponse(List<(CartItem CartItem, Domain.Food.Food Food)> addedCartItems)
    {
        return addedCartItems
            .Select(aci => ToCartItemResponse(aci.CartItem, aci.Food))
            .ToList();
    }

    private CartItemResponse ToCartItemResponse(CartItem cartItem, Domain.Food.Food food)
    {
        return new CartItemResponse
        {
            Id = cartItem.Id,
            FoodId = cartItem.FoodId,
            Name = food.Name,
            Description = food.Description,
            Price = food.Price,
            DiscountPrice = food.DiscountPrice,
            ImageUrl = GetImageUrl(food.Images),
            Category = food.Category.GetStringValue(),
            Quantity = cartItem.Quantity,
            StockQuantity = food.StockQuantity,
            QuantityUnit = food.QuantityUnit.GetStringValue(),
        };
    }

    private string GetImageUrl(IReadOnlyCollection<Domain.Food.FoodImage> images)
    {
        var firstImage = images.OrderBy(img => img.DisplayOrder).FirstOrDefault();
        return firstImage == null ? string.Empty : _storageService.GetPublicUrl(firstImage.FileKey);
    }
}