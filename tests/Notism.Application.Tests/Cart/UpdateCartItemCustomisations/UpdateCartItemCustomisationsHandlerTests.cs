using System.Reflection;

using FluentAssertions;

using Microsoft.Extensions.Logging;

using Notism.Application.Cart.UpdateCartItemCustomisations;
using Notism.Application.Common.Services;
using Notism.Domain.Cart;
using Notism.Domain.Common.Interfaces;
using Notism.Domain.Common.Specifications;
using Notism.Domain.Food;
using Notism.Domain.Food.Enums;
using Notism.Shared.Exceptions;

using NSubstitute;

namespace Notism.Application.Tests.Cart.UpdateCartItemCustomisations;

public class UpdateCartItemCustomisationsHandlerTests
{
    private readonly ICartItemRepository _cartItemRepository;
    private readonly IRepository<FoodCustomisationOption> _optionRepository;
    private readonly ILogger<UpdateCartItemCustomisationsHandler> _logger;
    private readonly IMessages _messages;
    private readonly UpdateCartItemCustomisationsHandler _handler;

    public UpdateCartItemCustomisationsHandlerTests()
    {
        _cartItemRepository = Substitute.For<ICartItemRepository>();
        _optionRepository = Substitute.For<IRepository<FoodCustomisationOption>>();
        _logger = Substitute.For<ILogger<UpdateCartItemCustomisationsHandler>>();
        _messages = Substitute.For<IMessages>();

        _messages.CartItemNotFound.Returns("Cart item not found");
        _messages.CustomisationOptionNotFound.Returns("Customisation option not found");
        _messages.CartItemNotBelongToUser.Returns("Cart item does not belong to the user");

        _handler = new UpdateCartItemCustomisationsHandler(
            _cartItemRepository,
            _optionRepository,
            _logger,
            _messages);
    }

    [Fact]
    public async Task Handle_WhenValidRequest_ClearsAndRecreatesCustomisations()
    {
        var userId = Guid.NewGuid();
        var food = CreateFood();
        var group = FoodCustomisationGroup.Create(food.Id, "Size", true, 1);
        group.AddOption("Large", 5000m, 1);
        food.AddCustomisationGroup(group);
        var cartItem = CreateCartItemWithFood(userId, food);

        var option = group.Options.First();

        _cartItemRepository
            .FindByExpressionAsync(Arg.Any<ISpecification<CartItem>>())
            .Returns(cartItem);

        _optionRepository
            .FilterByExpressionAsync(Arg.Any<FilterSpecification<FoodCustomisationOption>>())
            .Returns(new List<FoodCustomisationOption> { option });

        var request = new UpdateCartItemCustomisationsRequest
        {
            CartItemId = cartItem.Id,
            UserId = userId,
            Customisations = new List<CartItemCustomisationSelection>
            {
                new CartItemCustomisationSelection { GroupId = group.Id, OptionId = option.Id },
            },
        };

        var result = await _handler.Handle(request, CancellationToken.None);

        result.Should().NotBeNull();
        result.Id.Should().Be(cartItem.Id);
        cartItem.Customisations.Should().HaveCount(1);
        cartItem.Customisations.First().CustomisationOptionId.Should().Be(option.Id);

        await _cartItemRepository.Received(1).SaveChangesAsync();
    }

    [Fact]
    public async Task Handle_WhenCartItemNotFound_ThrowsNotFoundException()
    {
        _cartItemRepository
            .FindByExpressionAsync(Arg.Any<ISpecification<CartItem>>())
            .Returns((CartItem?)null);

        var request = new UpdateCartItemCustomisationsRequest
        {
            CartItemId = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            Customisations = new List<CartItemCustomisationSelection>
            {
                new CartItemCustomisationSelection { GroupId = Guid.NewGuid(), OptionId = Guid.NewGuid() },
            },
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
            .FindByExpressionAsync(Arg.Any<ISpecification<CartItem>>())
            .Returns(cartItem);

        var request = new UpdateCartItemCustomisationsRequest
        {
            CartItemId = cartItem.Id,
            UserId = Guid.NewGuid(),
            Customisations = new List<CartItemCustomisationSelection>
            {
                new CartItemCustomisationSelection { GroupId = Guid.NewGuid(), OptionId = Guid.NewGuid() },
            },
        };

        var act = async () => await _handler.Handle(request, CancellationToken.None);

        await act.Should().ThrowAsync<ForbiddenException>();
    }

    [Fact]
    public async Task Handle_WhenOptionNotFound_ThrowsNotFoundException()
    {
        var userId = Guid.NewGuid();
        var cartItem = CreateCartItem(userId, Guid.NewGuid());

        _cartItemRepository
            .FindByExpressionAsync(Arg.Any<ISpecification<CartItem>>())
            .Returns(cartItem);

        _optionRepository
            .FindByExpressionAsync(Arg.Any<FilterSpecification<FoodCustomisationOption>>())
            .Returns((FoodCustomisationOption?)null);

        var request = new UpdateCartItemCustomisationsRequest
        {
            CartItemId = cartItem.Id,
            UserId = userId,
            Customisations = new List<CartItemCustomisationSelection>
            {
                new CartItemCustomisationSelection { GroupId = Guid.NewGuid(), OptionId = Guid.NewGuid() },
            },
        };

        var act = async () => await _handler.Handle(request, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public void Validator_WhenDuplicateGroupIdInRequest_FailsValidation()
    {
        // Duplicate-group rejection lives in the request validator, not the handler.
        var duplicateGroupId = Guid.NewGuid();
        var validator = new UpdateCartItemCustomisationsRequestValidator();

        var request = new UpdateCartItemCustomisationsRequest
        {
            CartItemId = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            Customisations = new List<CartItemCustomisationSelection>
            {
                new CartItemCustomisationSelection { GroupId = duplicateGroupId, OptionId = Guid.NewGuid() },
                new CartItemCustomisationSelection { GroupId = duplicateGroupId, OptionId = Guid.NewGuid() },
            },
        };

        var result = validator.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "Duplicate group IDs are not allowed in a single request");
    }

    private static Domain.Food.Food CreateFood()
        => Domain.Food.Food.Create("Pizza", "Delicious pizza", 100000m, Guid.NewGuid(), QuantityUnit.Grams, 5);

    private static CartItem CreateCartItem(Guid userId, Guid foodId)
    {
        var cartItem = CartItem.Create(userId, foodId, 1);
        return cartItem;
    }

    private static CartItem CreateCartItemWithFood(Guid userId, Domain.Food.Food food)
    {
        var cartItem = CartItem.Create(userId, food.Id, 1);

        // Set the Food navigation property via reflection so handler mapping works in tests
        var foodProp = typeof(CartItem).GetProperty(nameof(CartItem.Food), BindingFlags.Public | BindingFlags.Instance)!;
        foodProp.SetValue(cartItem, food);
        return cartItem;
    }
}