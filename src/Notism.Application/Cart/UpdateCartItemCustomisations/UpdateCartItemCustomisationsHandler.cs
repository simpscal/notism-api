using MediatR;

using Microsoft.Extensions.Logging;

using Notism.Application.Cart.Common;
using Notism.Application.Common.Services;
using Notism.Domain.Cart;
using Notism.Domain.Cart.Repositories;
using Notism.Domain.Common.Repositories;
using Notism.Domain.Common.Specifications;
using Notism.Domain.Food;
using Notism.Shared.Exceptions;

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
        var cartItemSpec = new CartItemDetailSpecification(c => c.Id == request.CartItemId);
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

        return UpdateCartItemCustomisationsResponse.FromDomain(cartItem);
    }
}