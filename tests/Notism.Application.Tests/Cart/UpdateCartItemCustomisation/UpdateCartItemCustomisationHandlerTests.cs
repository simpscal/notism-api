using FluentAssertions;

using Microsoft.Extensions.Logging;

using Notism.Application.Cart.UpdateCartItemCustomisation;
using Notism.Application.Common.Services;
using Notism.Domain.Cart;
using Notism.Domain.Common.Interfaces;
using Notism.Domain.Common.Specifications;
using Notism.Domain.Food;
using Notism.Domain.Food.Enums;
using Notism.Shared.Exceptions;

using NSubstitute;

namespace Notism.Application.Tests.Cart.UpdateCartItemCustomisation;

public class UpdateCartItemCustomisationHandlerTests
{
    private readonly ICartItemRepository _cartItemRepository;
    private readonly IRepository<FoodCustomisationOption> _optionRepository;
    private readonly ILogger<UpdateCartItemCustomisationHandler> _logger;
    private readonly IMessages _messages;
    private readonly UpdateCartItemCustomisationHandler _handler;

    public UpdateCartItemCustomisationHandlerTests()
    {
        _cartItemRepository = Substitute.For<ICartItemRepository>();
        _optionRepository = Substitute.For<IRepository<FoodCustomisationOption>>();
        _logger = Substitute.For<ILogger<UpdateCartItemCustomisationHandler>>();
        _messages = Substitute.For<IMessages>();

        _messages.CartItemNotFound.Returns("Cart item not found");
        _messages.CustomisationOptionNotFound.Returns("Customisation option not found");
        _messages.CartItemNotBelongToUser.Returns("Cart item does not belong to the user");

        _handler = new UpdateCartItemCustomisationHandler(
            _cartItemRepository,
            _optionRepository,
            _logger,
            _messages);
    }

    [Fact]
    public async Task Handle_WhenValidRequest_UpdatesCustomisationAndReturnsResponse()
    {
        var userId = Guid.NewGuid();
        var food = CreateFood();
        var cartItem = CreateCartItem(userId, food.Id);

        var group = FoodCustomisationGroup.Create(food.Id, "Size", true, 1);
        group.AddOption("Large", 5000m, 1);
        food.AddCustomisationGroup(group);

        var option = group.Options.First();

        _cartItemRepository
            .FindByExpressionAsync(Arg.Any<FilterSpecification<CartItem>>())
            .Returns(cartItem);

        _optionRepository
            .FindByExpressionAsync(Arg.Any<FilterSpecification<FoodCustomisationOption>>())
            .Returns(option);

        var request = new UpdateCartItemCustomisationRequest
        {
            CartItemId = cartItem.Id,
            UserId = userId,
            CustomisationOptionId = option.Id,
        };

        var result = await _handler.Handle(request, CancellationToken.None);

        result.Should().NotBeNull();
        result.Id.Should().Be(cartItem.Id);
        cartItem.CustomisationOptionId.Should().Be(option.Id);
        cartItem.CustomisationGroupId.Should().Be(group.Id);
        cartItem.CustomisationLabel.Should().Be("Large");
        cartItem.Surcharge.Should().Be(5000m);

        await _cartItemRepository.Received(1).SaveChangesAsync();
    }

    [Fact]
    public async Task Handle_WhenCartItemNotFound_ThrowsNotFoundException()
    {
        _cartItemRepository
            .FindByExpressionAsync(Arg.Any<FilterSpecification<CartItem>>())
            .Returns((CartItem?)null);

        var request = new UpdateCartItemCustomisationRequest
        {
            CartItemId = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            CustomisationOptionId = Guid.NewGuid(),
        };

        var act = async () => await _handler.Handle(request, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenCartItemBelongsToDifferentUser_ThrowsForbiddenException()
    {
        var foodId = Guid.NewGuid();
        var cartItem = CreateCartItem(Guid.NewGuid(), foodId);

        _cartItemRepository
            .FindByExpressionAsync(Arg.Any<FilterSpecification<CartItem>>())
            .Returns(cartItem);

        var request = new UpdateCartItemCustomisationRequest
        {
            CartItemId = cartItem.Id,
            UserId = Guid.NewGuid(),
            CustomisationOptionId = Guid.NewGuid(),
        };

        var act = async () => await _handler.Handle(request, CancellationToken.None);

        await act.Should().ThrowAsync<ForbiddenException>();
    }

    [Fact]
    public async Task Handle_WhenOptionNotFoundOrNotBelongingToFood_ThrowsNotFoundException()
    {
        var userId = Guid.NewGuid();
        var cartItem = CreateCartItem(userId, Guid.NewGuid());

        _cartItemRepository
            .FindByExpressionAsync(Arg.Any<FilterSpecification<CartItem>>())
            .Returns(cartItem);

        _optionRepository
            .FindByExpressionAsync(Arg.Any<FilterSpecification<FoodCustomisationOption>>())
            .Returns((FoodCustomisationOption?)null);

        var request = new UpdateCartItemCustomisationRequest
        {
            CartItemId = cartItem.Id,
            UserId = userId,
            CustomisationOptionId = Guid.NewGuid(),
        };

        var act = async () => await _handler.Handle(request, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    private static Domain.Food.Food CreateFood()
        => Domain.Food.Food.Create("Pizza", "Delicious pizza", 100000m, Guid.NewGuid(), QuantityUnit.Grams, 5);

    private static CartItem CreateCartItem(Guid userId, Guid foodId)
        => CartItem.Create(userId, foodId, 1);
}
