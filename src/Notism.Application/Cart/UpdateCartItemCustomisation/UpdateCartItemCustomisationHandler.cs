using MediatR;

using Microsoft.Extensions.Logging;

using Notism.Application.Common.Services;
using Notism.Domain.Cart;
using Notism.Domain.Common.Interfaces;
using Notism.Domain.Common.Specifications;
using Notism.Domain.Food;
using Notism.Shared.Exceptions;

namespace Notism.Application.Cart.UpdateCartItemCustomisation;

public class UpdateCartItemCustomisationHandler : IRequestHandler<UpdateCartItemCustomisationRequest, UpdateCartItemCustomisationResponse>
{
    private readonly ICartItemRepository _cartItemRepository;
    private readonly IRepository<FoodCustomisationOption> _optionRepository;
    private readonly ILogger<UpdateCartItemCustomisationHandler> _logger;
    private readonly IMessages _messages;

    public UpdateCartItemCustomisationHandler(
        ICartItemRepository cartItemRepository,
        IRepository<FoodCustomisationOption> optionRepository,
        ILogger<UpdateCartItemCustomisationHandler> logger,
        IMessages messages)
    {
        _cartItemRepository = cartItemRepository;
        _optionRepository = optionRepository;
        _logger = logger;
        _messages = messages;
    }

    public async Task<UpdateCartItemCustomisationResponse> Handle(
        UpdateCartItemCustomisationRequest request,
        CancellationToken cancellationToken)
    {
        var cartItemSpec = new FilterSpecification<CartItem>(c => c.Id == request.CartItemId);
        var cartItem = await _cartItemRepository.FindByExpressionAsync(cartItemSpec)
            ?? throw new NotFoundException(_messages.CartItemNotFound);

        if (cartItem.UserId != request.UserId)
        {
            throw new ForbiddenException(_messages.CartItemNotBelongToUser);
        }

        var optionSpec = new FilterSpecification<FoodCustomisationOption>(
            o => o.Id == request.CustomisationOptionId && o.Group.FoodId == cartItem.FoodId)
            .Include(o => o.Group);
        var option = await _optionRepository.FindByExpressionAsync(optionSpec)
            ?? throw new NotFoundException(_messages.CustomisationOptionNotFound);

        cartItem.SetCustomisation(option.GroupId, option.Id, option.Label, option.Surcharge);
        await _cartItemRepository.SaveChangesAsync();

        _logger.LogInformation(
            "Updated customisation for cart item {CartItemId} to option {OptionId} for user {UserId}",
            cartItem.Id,
            option.Id,
            request.UserId);

        return new UpdateCartItemCustomisationResponse { Id = cartItem.Id };
    }
}
