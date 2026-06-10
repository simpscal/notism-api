using MediatR;

using Microsoft.Extensions.Logging;

using Notism.Application.Common.Services;
using Notism.Domain.Cart;
using Notism.Domain.Cart.Repositories;
using Notism.Domain.Common.Repositories;
using Notism.Domain.Common.Specifications;
using Notism.Domain.Food;
using Notism.Shared.Exceptions;

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
            .Include(f => f.Category!)
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
            var explicitOptions = await ResolveCustomisationOptionsAsync(food.Id);
            var allOptions = explicitOptions.Concat(GetRequiredGroupDefaults(food, explicitOptions.Select(o => o.GroupId))).ToList();
            var groupLabels = food.CustomisationGroups.ToDictionary(g => g.Id, g => g.Label);
            existingCartItem.ClearCustomisations();
            foreach (var option in allOptions)
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

        return AddCartItemResponse.FromDomain(existingCartItem, existingCartItem.Food, food);
    }

    private async Task<AddCartItemResponse> CreateNewCartItemAsync(Domain.Food.Food food)
    {
        if (_request!.Quantity > food.StockQuantity)
        {
            throw new ResultFailureException(_messages.InsufficientStock);
        }

        var explicitOptions = await ResolveCustomisationOptionsAsync(food.Id);
        var allOptions = explicitOptions.Concat(GetRequiredGroupDefaults(food, explicitOptions.Select(o => o.GroupId))).ToList();

        var groupLabels = food.CustomisationGroups.ToDictionary(g => g.Id, g => g.Label);
        var cartItem = CartItem.Create(_request.UserId, _request.FoodId, _request.Quantity);

        foreach (var option in allOptions)
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

        return AddCartItemResponse.FromDomain(cartItem, food, food);
    }

    private static IEnumerable<FoodCustomisationOption> GetRequiredGroupDefaults(
        Domain.Food.Food food,
        IEnumerable<Guid> coveredGroupIds)
    {
        var covered = coveredGroupIds.ToHashSet();

        return food.CustomisationGroups
            .Where(g => g.IsRequired && !covered.Contains(g.Id))
            .OrderBy(g => g.DisplayOrder)
            .Select(g => g.Options.OrderBy(o => o.DisplayOrder).FirstOrDefault())
            .OfType<FoodCustomisationOption>();
    }
}