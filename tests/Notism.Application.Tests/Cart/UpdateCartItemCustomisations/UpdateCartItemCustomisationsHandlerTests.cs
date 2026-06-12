using System.Reflection;

using FluentAssertions;

using Microsoft.Extensions.Logging;

using Notism.Application.Cart.UpdateCartItemCustomisations;
using Notism.Application.Common.Services;
using Notism.Application.Tests.Common;
using Notism.Domain.Cart;
using Notism.Domain.Food;
using Notism.Domain.Food.Enums;
using Notism.Infrastructure.Repositories;
using Notism.Shared.Exceptions;

using NSubstitute;

namespace Notism.Application.Tests.Cart.UpdateCartItemCustomisations;

public class UpdateCartItemCustomisationsHandlerTests : IDisposable
{
    private readonly WriteHandlerContext _context;
    private readonly IMessages _messages;
    private readonly UpdateCartItemCustomisationsHandler _handler;

    public UpdateCartItemCustomisationsHandlerTests()
    {
        _context = new WriteHandlerContext();
        _messages = Substitute.For<IMessages>();

        _messages.CartItemNotFound.Returns("Cart item not found");
        _messages.CustomisationOptionNotFound.Returns("Customisation option not found");
        _messages.CartItemNotBelongToUser.Returns("Cart item does not belong to the user");

        _handler = new UpdateCartItemCustomisationsHandler(
            new CartItemRepository(_context.DbContext),
            _context.DbContext,
            Substitute.For<ILogger<UpdateCartItemCustomisationsHandler>>(),
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
        var cartItem = CreateCartItem(userId, food);
        await _context.SeedAsync(food, cartItem);

        var option = group.Options.First();

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

        _context.DbContext.ChangeTracker.Clear();
        var persisted = _context.DbContext.CartItemCustomisations
            .Where(c => c.CartItemId == cartItem.Id)
            .ToList();
        persisted.Should().ContainSingle(c => c.CustomisationOptionId == option.Id);
    }

    [Fact]
    public async Task Handle_WhenCartItemNotFound_ThrowsNotFoundException()
    {
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
        var food = CreateFood();
        var cartItem = CreateCartItem(Guid.NewGuid(), food);
        await _context.SeedAsync(food, cartItem);

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
        var food = CreateFood();
        var cartItem = CreateCartItem(userId, food);
        await _context.SeedAsync(food, cartItem);

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

    public void Dispose()
        => _context.Dispose();

    private static Domain.Food.Food CreateFood()
        => Domain.Food.Food.Create("Pizza", "Delicious pizza", 100000m, Guid.NewGuid(), QuantityUnit.Grams, 5);

    // EF InMemory inner-joins a required reference Include; link the cart item's Food
    // navigation so the seeded item survives the handler's graph load.
    private static CartItem CreateCartItem(Guid userId, Domain.Food.Food food)
    {
        var cartItem = CartItem.Create(userId, food.Id, 1);
        typeof(CartItem)
            .GetProperty(nameof(CartItem.Food), BindingFlags.Public | BindingFlags.Instance)!
            .SetValue(cartItem, food);
        return cartItem;
    }
}
