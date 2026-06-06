using MediatR;

using Microsoft.Extensions.Logging;

using Notism.Application.Cart.Common;
using Notism.Application.Common.Constants;
using Notism.Application.Common.Interfaces;
using Notism.Application.Common.Services;
using Notism.Domain.Cart;
using Notism.Domain.Common.Interfaces;
using Notism.Domain.Common.Specifications;
using Notism.Domain.Food;
using Notism.Shared.Exceptions;
using Notism.Shared.Extensions;

namespace Notism.Application.Cart.UpdateCartItemCustomisations;

public class UpdateCartItemCustomisationsHandler : IRequestHandler<UpdateCartItemCustomisationsRequest, UpdateCartItemCustomisationsResponse>
{
    private readonly ICartItemRepository _cartItemRepository;
    private readonly IRepository<FoodCustomisationOption> _optionRepository;
    private readonly ILogger<UpdateCartItemCustomisationsHandler> _logger;
    private readonly IMessages _messages;

    public UpdateCartItemCustomisationsHandler(
        ICartItemRepository cartItemRepository,
        IRepository<FoodCustomisationOption> optionRepository,
        ILogger<UpdateCartItemCustomisationsHandler> logger,
        IMessages messages)
    {
        _cartItemRepository = cartItemRepository;
        _optionRepository = optionRepository;
        _logger = logger;
        _messages = messages;
    }

    public async Task<UpdateCartItemCustomisationsResponse> Handle(
        UpdateCartItemCustomisationsRequest request,
        CancellationToken cancellationToken)
    {
        var cartItemSpec = new FilterSpecification<CartItem>(c => c.Id == request.CartItemId)
            .Include(c => c.Food)
            .Include("Food.Category")
            .Include(c => c.Food.Images.OrderBy(i => i.DisplayOrder).Take(1))
            .Include("Food.CustomisationGroups.Options")
            .Include(c => c.Customisations);
        var cartItem = await _cartItemRepository.FindByExpressionAsync(cartItemSpec)
            ?? throw new NotFoundException(_messages.CartItemNotFound);

        if (cartItem.UserId != request.UserId)
        {
            throw new ForbiddenException(_messages.CartItemNotBelongToUser);
        }

        // Resolve all options in a single query
        var requestedOptionIds = request.Customisations.Select(c => c.OptionId).ToList();
        var optionSpec = new FilterSpecification<FoodCustomisationOption>(
            o => requestedOptionIds.Contains(o.Id) && o.Group.FoodId == cartItem.FoodId)
            .Include(o => o.Group);
        var fetchedOptions = (await _optionRepository.FilterByExpressionAsync(optionSpec))
            .ToDictionary(o => o.Id);

        var resolvedOptions = new List<(FoodCustomisationOption Option, string GroupLabel)>();
        foreach (var selection in request.Customisations)
        {
            if (!fetchedOptions.TryGetValue(selection.OptionId, out var option) || option.GroupId != selection.GroupId)
            {
                throw new NotFoundException(_messages.CustomisationOptionNotFound);
            }

            var groupLabel = cartItem.Food.CustomisationGroups
                .FirstOrDefault(g => g.Id == option.GroupId)?.Label
                ?? option.Group.Label;

            resolvedOptions.Add((option, groupLabel));
        }

        cartItem.ClearCustomisations();

        foreach (var (option, groupLabel) in resolvedOptions)
        {
            cartItem.AddCustomisation(option.GroupId, option.Id, groupLabel, option.Label, option.Surcharge);
        }

        await _cartItemRepository.SaveChangesAsync();

        _logger.LogInformation(
            "Updated customisations for cart item {CartItemId} for user {UserId}",
            cartItem.Id,
            request.UserId);

        return MapToResponse(cartItem);
    }

    private static UpdateCartItemCustomisationsResponse MapToResponse(CartItem cartItem)
    {
        var customisations = cartItem.Customisations.Select(c =>
        {
            // Find the group from food's customisation groups to build available options
            var group = cartItem.Food.CustomisationGroups
                .FirstOrDefault(g => g.Id == c.CustomisationGroupId);

            var availableOptions = group?.Options
                .Select(CartItemAvailableOptionResponse.FromDomain)
                .ToList() ?? new List<CartItemAvailableOptionResponse>();

            // Orphan check: if stored option no longer exists
            var optionStillExists = group?.Options.Any(o => o.Id == c.CustomisationOptionId) ?? false;

            return new CartItemCustomisationResponse
            {
                GroupId = c.CustomisationGroupId,
                GroupLabel = c.GroupLabel,
                OptionId = optionStillExists ? c.CustomisationOptionId : null,
                OptionLabel = optionStillExists ? c.OptionLabel : "Option no longer available",
                Surcharge = optionStillExists ? c.Surcharge : null,
                AvailableOptions = availableOptions,
            };
        }).ToList();

        return new UpdateCartItemCustomisationsResponse
        {
            Id = cartItem.Id,
            FoodId = cartItem.FoodId,
            Name = cartItem.Food.Name,
            Description = cartItem.Food.Description,
            Price = cartItem.Food.Price,
            DiscountPrice = cartItem.Food.DiscountPrice,
            ImageUrl = cartItem.Food.Images.FirstOrDefault()?.FileKey ?? string.Empty,
            Category = cartItem.Food.Category?.Name ?? string.Empty,
            Quantity = cartItem.Quantity,
            StockQuantity = cartItem.Food.StockQuantity,
            QuantityUnit = cartItem.Food.QuantityUnit.GetStringValue(),
            Customisations = customisations,
            TotalSurcharge = cartItem.TotalSurcharge,
        };
    }
}