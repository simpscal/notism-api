using MediatR;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using Notism.Application.Common.Persistence;
using Notism.Application.Common.Services;
using Notism.Domain.Cart;
using Notism.Domain.Cart.Repositories;
using Notism.Domain.Food;
using Notism.Shared.Exceptions;

namespace Notism.Application.Cart.UpdateCartItemCustomisations;

public class UpdateCartItemCustomisationsHandler : IRequestHandler<UpdateCartItemCustomisationsRequest, UpdateCartItemCustomisationsResponse>
{
    private readonly ICartItemRepository _cartItemRepository;
    private readonly IReadDbContext _readDbContext;
    private readonly ILogger<UpdateCartItemCustomisationsHandler> _logger;
    private readonly IMessages _messages;

    public UpdateCartItemCustomisationsHandler(
        ICartItemRepository cartItemRepository,
        IReadDbContext readDbContext,
        ILogger<UpdateCartItemCustomisationsHandler> logger,
        IMessages messages)
    {
        _cartItemRepository = cartItemRepository;
        _readDbContext = readDbContext;
        _logger = logger;
        _messages = messages;
    }

    public async Task<UpdateCartItemCustomisationsResponse> Handle(
        UpdateCartItemCustomisationsRequest request,
        CancellationToken cancellationToken)
    {
        var cartItem = await _readDbContext.Set<CartItem>(tracking: true)
            .Where(c => c.Id == request.CartItemId)
            .Include(c => c.Food)
            .Include("Food.Category")
            .Include(c => c.Food.Images.OrderBy(i => i.DisplayOrder).Take(1))
            .Include("Food.CustomisationGroups.Options")
            .Include(c => c.Customisations)
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new NotFoundException(_messages.CartItemNotFound);

        if (cartItem.UserId != request.UserId)
        {
            throw new ForbiddenException(_messages.CartItemNotBelongToUser);
        }

        // Resolve all options in a single query
        var requestedOptionIds = request.Customisations.Select(c => c.OptionId).ToList();
        var fetchedOptions = (await _readDbContext.Set<FoodCustomisationOption>(tracking: true)
            .Where(o => requestedOptionIds.Contains(o.Id) && o.Group.FoodId == cartItem.FoodId)
            .Include(o => o.Group)
            .ToListAsync(cancellationToken))
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