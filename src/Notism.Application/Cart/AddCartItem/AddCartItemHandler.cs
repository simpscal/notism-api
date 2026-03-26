using MediatR;

using Microsoft.Extensions.Logging;

using Notism.Application.Common.Constants;
using Notism.Application.Common.Interfaces;
using Notism.Application.Common.Services;
using Notism.Domain.Cart;
using Notism.Domain.Common.Interfaces;
using Notism.Domain.Common.Specifications;
using Notism.Shared.Exceptions;
using Notism.Shared.Extensions;

namespace Notism.Application.Cart.AddCartItem;

public class AddCartItemHandler : IRequestHandler<AddCartItemRequest, AddCartItemResponse>
{
    private readonly ICartItemRepository _cartItemRepository;
    private readonly IRepository<Domain.Food.Food> _foodRepository;
    private readonly IStorageService _storageService;
    private readonly ILogger<AddCartItemHandler> _logger;
    private readonly IMessages _messages;
    private AddCartItemRequest? _request;

    public AddCartItemHandler(
        ICartItemRepository cartItemRepository,
        IRepository<Domain.Food.Food> foodRepository,
        IStorageService storageService,
        ILogger<AddCartItemHandler> logger,
        IMessages messages)
    {
        _cartItemRepository = cartItemRepository;
        _foodRepository = foodRepository;
        _storageService = storageService;
        _logger = logger;
        _messages = messages;
    }

    public async Task<AddCartItemResponse> Handle(
        AddCartItemRequest request,
        CancellationToken cancellationToken)
    {
        _request = request;

        var food = await ValidateAndFetchFoodAsync();
        var existingCartItem = await GetExistingCartItemAsync();

        if (existingCartItem != null)
        {
            return await UpdateExistingCartItemAsync(existingCartItem, food);
        }

        return await CreateNewCartItemAsync(food);
    }

    private async Task<Domain.Food.Food> ValidateAndFetchFoodAsync()
    {
        var foodSpecification = new FilterSpecification<Domain.Food.Food>(f => f.Id == _request!.FoodId)
            .Include("Category")
            .Include(f => f.Images.OrderBy(i => i.DisplayOrder).Take(1));
        var food = await _foodRepository.FindByExpressionAsync(foodSpecification)
            ?? throw new ResultFailureException(_messages.FoodNotFound);

        if (!food.IsAvailable)
        {
            throw new ResultFailureException(_messages.FoodNotAvailable);
        }

        return food;
    }

    private async Task<CartItem?> GetExistingCartItemAsync()
    {
        var cartItemSpecification = new FilterSpecification<CartItem>(c => c.UserId == _request!.UserId && c.FoodId == _request.FoodId)
            .Include(c => c.Food)
            .Include("Food.Category")
            .Include(c => c.Food.Images.OrderBy(i => i.DisplayOrder).Take(1));
        return await _cartItemRepository.FindByExpressionAsync(cartItemSpecification);
    }

    private async Task<AddCartItemResponse> UpdateExistingCartItemAsync(
        CartItem existingCartItem,
        Domain.Food.Food food)
    {
        var newQuantity = existingCartItem.Quantity + _request!.Quantity;
        if (newQuantity > food.StockQuantity)
        {
            throw new ResultFailureException(_messages.InsufficientStock);
        }

        existingCartItem.UpdateQuantity(newQuantity);
        await _cartItemRepository.SaveChangesAsync();

        _logger.LogInformation(
            "Updated cart item {CartItemId} quantity to {Quantity} for user {UserId}",
            existingCartItem.Id,
            newQuantity,
            existingCartItem.UserId);

        return new AddCartItemResponse
        {
            Id = existingCartItem.Id,
            FoodId = existingCartItem.FoodId,
            Name = existingCartItem.Food.Name,
            Description = existingCartItem.Food.Description,
            Price = existingCartItem.Food.Price,
            DiscountPrice = existingCartItem.Food.DiscountPrice,
            ImageUrl = GetImageUrl(existingCartItem.Food.Images),
            Category = existingCartItem.Food.Category?.Name ?? string.Empty,
            Quantity = existingCartItem.Quantity,
            StockQuantity = existingCartItem.Food.StockQuantity,
            QuantityUnit = existingCartItem.Food.QuantityUnit.GetStringValue(),
        };
    }

    private async Task<AddCartItemResponse> CreateNewCartItemAsync(Domain.Food.Food food)
    {
        if (_request!.Quantity > food.StockQuantity)
        {
            throw new ResultFailureException(_messages.InsufficientStock);
        }

        var cartItem = CartItem.Create(_request.UserId, _request.FoodId, _request.Quantity);
        _cartItemRepository.Add(cartItem);

        await _cartItemRepository.SaveChangesAsync();

        _logger.LogInformation(
            "Added cart item {CartItemId} for user {UserId}",
            cartItem.Id,
            _request.UserId);

        return new AddCartItemResponse
        {
            Id = cartItem.Id,
            FoodId = cartItem.FoodId,
            Name = food.Name,
            Description = food.Description,
            Price = food.Price,
            DiscountPrice = food.DiscountPrice,
            ImageUrl = GetImageUrl(food.Images),
            Category = food.Category?.Name ?? string.Empty,
            Quantity = cartItem.Quantity,
            StockQuantity = food.StockQuantity,
            QuantityUnit = food.QuantityUnit.GetStringValue(),
        };
    }

    private string GetImageUrl(IReadOnlyCollection<Domain.Food.FoodImage> images)
    {
        var firstImage = images.OrderBy(img => img.DisplayOrder).FirstOrDefault();
        return firstImage == null ? string.Empty : _storageService.GetPublicUrl(firstImage.FileKey, StorageTypeConstants.Food);
    }
}