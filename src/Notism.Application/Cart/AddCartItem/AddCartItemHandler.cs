using MediatR;

using Microsoft.Extensions.Logging;

using Notism.Application.Cart.Models;
using Notism.Application.Common.Constants;
using Notism.Application.Common.Interfaces;
using Notism.Application.Common.Services;
using Notism.Domain.Cart;
using Notism.Domain.Common.Interfaces;
using Notism.Domain.Common.Specifications;
using Notism.Domain.Food;
using Notism.Shared.Exceptions;
using Notism.Shared.Extensions;

namespace Notism.Application.Cart.AddCartItem;

public class AddCartItemHandler : IRequestHandler<AddCartItemRequest, AddCartItemResponse>
{
    private readonly ICartItemRepository _cartItemRepository;
    private readonly IRepository<Domain.Food.Food> _foodRepository;
    private readonly IRepository<FoodCustomisationOption> _optionRepository;
    private readonly IStorageService _storageService;
    private readonly ILogger<AddCartItemHandler> _logger;
    private readonly IMessages _messages;
    private AddCartItemRequest? _request;

    public AddCartItemHandler(
        ICartItemRepository cartItemRepository,
        IRepository<Domain.Food.Food> foodRepository,
        IRepository<FoodCustomisationOption> optionRepository,
        IStorageService storageService,
        ILogger<AddCartItemHandler> logger,
        IMessages messages)
    {
        _cartItemRepository = cartItemRepository;
        _foodRepository = foodRepository;
        _optionRepository = optionRepository;
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
            .Include(f => f.Images.OrderBy(i => i.DisplayOrder).Take(1))
            .Include("CustomisationGroups.Options");
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

    private async Task<List<FoodCustomisationOption>> ResolveCustomisationOptionsAsync(Guid foodId)
    {
        if (_request!.Customisations == null || _request.Customisations.Count == 0)
        {
            return new List<FoodCustomisationOption>();
        }

        var requestedOptionIds = _request.Customisations.Select(c => c.OptionId).ToList();
        var optionSpec = new FilterSpecification<FoodCustomisationOption>(
            o => requestedOptionIds.Contains(o.Id) && o.Group.FoodId == foodId)
            .Include(o => o.Group);
        var fetched = (await _optionRepository.FilterByExpressionAsync(optionSpec))
            .ToDictionary(o => o.Id);

        var resolved = new List<FoodCustomisationOption>();
        foreach (var selection in _request.Customisations)
        {
            if (!fetched.TryGetValue(selection.OptionId, out var option) || option.GroupId != selection.GroupId)
            {
                throw new ResultFailureException(_messages.CustomisationOptionNotFound);
            }

            resolved.Add(option);
        }

        return resolved;
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

        if (_request.Customisations != null && _request.Customisations.Count > 0)
        {
            var options = await ResolveCustomisationOptionsAsync(food.Id);
            FillRequiredGroupDefaults(food, options);
            var groupLabels = food.CustomisationGroups.ToDictionary(g => g.Id, g => g.Label);
            existingCartItem.ClearCustomisations();
            foreach (var option in options)
            {
                var groupLabel = option.Group?.Label ?? groupLabels.GetValueOrDefault(option.GroupId, string.Empty);
                existingCartItem.AddCustomisation(option.GroupId, option.Id, groupLabel, option.Label, option.Surcharge);
            }
        }

        await _cartItemRepository.SaveChangesAsync();

        _logger.LogInformation(
            "Updated cart item {CartItemId} quantity to {Quantity} for user {UserId}",
            existingCartItem.Id,
            newQuantity,
            existingCartItem.UserId);

        return BuildResponse(existingCartItem, existingCartItem.Food, food);
    }

    private async Task<AddCartItemResponse> CreateNewCartItemAsync(Domain.Food.Food food)
    {
        if (_request!.Quantity > food.StockQuantity)
        {
            throw new ResultFailureException(_messages.InsufficientStock);
        }

        var options = await ResolveCustomisationOptionsAsync(food.Id);
        FillRequiredGroupDefaults(food, options);

        var groupLabels = food.CustomisationGroups.ToDictionary(g => g.Id, g => g.Label);
        var cartItem = CartItem.Create(_request.UserId, _request.FoodId, _request.Quantity);

        foreach (var option in options)
        {
            var groupLabel = option.Group?.Label ?? groupLabels.GetValueOrDefault(option.GroupId, string.Empty);
            cartItem.AddCustomisation(option.GroupId, option.Id, groupLabel, option.Label, option.Surcharge);
        }

        _cartItemRepository.Add(cartItem);
        await _cartItemRepository.SaveChangesAsync();

        _logger.LogInformation(
            "Added cart item {CartItemId} for user {UserId}",
            cartItem.Id,
            _request.UserId);

        return BuildResponse(cartItem, food, food);
    }

    private static AddCartItemResponse BuildResponse(CartItem cartItem, Domain.Food.Food cartFood, Domain.Food.Food fullFood)
    {
        return new AddCartItemResponse
        {
            Id = cartItem.Id,
            FoodId = cartItem.FoodId,
            Name = cartFood.Name,
            Description = cartFood.Description,
            Price = cartFood.Price,
            DiscountPrice = cartFood.DiscountPrice,
            ImageUrl = cartFood.Images.OrderBy(i => i.DisplayOrder).FirstOrDefault()?.FileKey ?? string.Empty,
            Category = cartFood.Category?.Name ?? string.Empty,
            Quantity = cartItem.Quantity,
            StockQuantity = cartFood.StockQuantity,
            QuantityUnit = cartFood.QuantityUnit.GetStringValue(),
            Customisations = cartItem.Customisations.Select(c =>
            {
                var group = fullFood.CustomisationGroups.FirstOrDefault(g => g.Id == c.CustomisationGroupId);
                return new CartItemCustomisationResponse
                {
                    GroupId = c.CustomisationGroupId,
                    GroupLabel = c.GroupLabel,
                    OptionId = c.CustomisationOptionId,
                    OptionLabel = c.OptionLabel,
                    Surcharge = c.Surcharge,
                    AvailableOptions = group?.Options
                        .OrderBy(o => o.DisplayOrder)
                        .Select(o => new CartItemAvailableOptionResponse
                        {
                            Id = o.Id,
                            Label = o.Label,
                            Surcharge = o.Surcharge,
                        })
                        .ToList() ?? new List<CartItemAvailableOptionResponse>(),
                };
            }).ToList(),
            TotalSurcharge = cartItem.TotalSurcharge,
        };
    }

    private static void FillRequiredGroupDefaults(Domain.Food.Food food, List<FoodCustomisationOption> resolved)
    {
        var coveredGroupIds = resolved.Select(o => o.GroupId).ToHashSet();

        foreach (var group in food.CustomisationGroups
                                   .Where(g => g.IsRequired && !coveredGroupIds.Contains(g.Id))
                                   .OrderBy(g => g.DisplayOrder))
        {
            var defaultOption = group.Options.OrderBy(o => o.DisplayOrder).FirstOrDefault();
            if (defaultOption != null)
            {
                resolved.Add(defaultOption);
            }
        }
    }
}
