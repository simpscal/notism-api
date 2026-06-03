using FluentAssertions;

using Microsoft.Extensions.Logging;

using Notism.Application.Cart.AddCartItem;
using Notism.Application.Common.Interfaces;
using Notism.Application.Common.Services;
using Notism.Domain.Cart;
using Notism.Domain.Common.Interfaces;
using Notism.Domain.Common.Specifications;
using Notism.Domain.Food;
using Notism.Domain.Food.Enums;

using NSubstitute;

namespace Notism.Application.Tests.Cart.AddCartItem;

public class AddCartItemHandlerDefaultCustomisationTests
{
    private readonly ICartItemRepository _cartItemRepository;
    private readonly IRepository<Domain.Food.Food> _foodRepository;
    private readonly IRepository<FoodCustomisationOption> _optionRepository;
    private readonly IStorageService _storageService;
    private readonly ILogger<AddCartItemHandler> _logger;
    private readonly IMessages _messages;
    private readonly AddCartItemHandler _handler;

    public AddCartItemHandlerDefaultCustomisationTests()
    {
        _cartItemRepository = Substitute.For<ICartItemRepository>();
        _foodRepository = Substitute.For<IRepository<Domain.Food.Food>>();
        _optionRepository = Substitute.For<IRepository<FoodCustomisationOption>>();
        _storageService = Substitute.For<IStorageService>();
        _logger = Substitute.For<ILogger<AddCartItemHandler>>();
        _messages = Substitute.For<IMessages>();

        _messages.FoodNotFound.Returns("Food not found");
        _messages.FoodNotAvailable.Returns("Food not available");
        _messages.InsufficientStock.Returns("Insufficient stock");

        _handler = new AddCartItemHandler(
            _cartItemRepository,
            _foodRepository,
            _optionRepository,
            _storageService,
            _logger,
            _messages);
    }

    [Fact]
    public async Task Handle_WhenNoCustomisationsProvided_DefaultsFirstOptionOfEachRequiredGroup()
    {
        var userId = Guid.NewGuid();
        var food = CreateFoodWithRequiredGroup(out var requiredGroup, out var defaultOption);

        _foodRepository
            .FindByExpressionAsync(Arg.Any<FilterSpecification<Domain.Food.Food>>())
            .Returns(food);

        _cartItemRepository
            .FindByExpressionAsync(Arg.Any<FilterSpecification<CartItem>>())
            .Returns((CartItem?)null);

        CartItem? addedItem = null;
        _cartItemRepository.When(r => r.Add(Arg.Any<CartItem>()))
            .Do(ci => addedItem = ci.Arg<CartItem>());

        var request = new AddCartItemRequest
        {
            UserId = userId,
            FoodId = food.Id,
            Quantity = 1,
        };

        await _handler.Handle(request, CancellationToken.None);

        await _cartItemRepository.Received(1).SaveChangesAsync();
        addedItem.Should().NotBeNull();
        addedItem!.Customisations.Should().HaveCount(1);
        addedItem.Customisations.First().CustomisationGroupId.Should().Be(requiredGroup.Id);
        addedItem.Customisations.First().CustomisationOptionId.Should().Be(defaultOption.Id);
    }

    [Fact]
    public async Task Handle_WhenRequiredGroupExplicitlyProvided_DoesNotOverrideWithDefault()
    {
        var userId = Guid.NewGuid();
        var food = CreateFoodWithRequiredGroup(out var requiredGroup, out _);
        var explicitOption = requiredGroup.Options.Last();

        _foodRepository
            .FindByExpressionAsync(Arg.Any<FilterSpecification<Domain.Food.Food>>())
            .Returns(food);

        _cartItemRepository
            .FindByExpressionAsync(Arg.Any<FilterSpecification<CartItem>>())
            .Returns((CartItem?)null);

        _optionRepository
            .FilterByExpressionAsync(Arg.Any<FilterSpecification<FoodCustomisationOption>>())
            .Returns(new List<FoodCustomisationOption> { explicitOption });

        CartItem? addedItem = null;
        _cartItemRepository.When(r => r.Add(Arg.Any<CartItem>()))
            .Do(ci => addedItem = ci.Arg<CartItem>());

        var request = new AddCartItemRequest
        {
            UserId = userId,
            FoodId = food.Id,
            Quantity = 1,
            Customisations = new List<CartItemCustomisationSelection>
            {
                new() { GroupId = requiredGroup.Id, OptionId = explicitOption.Id },
            },
        };

        await _handler.Handle(request, CancellationToken.None);

        addedItem.Should().NotBeNull();
        addedItem!.Customisations.Should().HaveCount(1);
        addedItem.Customisations.First().CustomisationOptionId.Should().Be(explicitOption.Id);
    }

    [Fact]
    public async Task Handle_WhenOptionalGroupNotProvided_DoesNotAddDefault()
    {
        var userId = Guid.NewGuid();
        var food = Domain.Food.Food.Create("Burger", "Tasty", 100000m, Guid.NewGuid(), QuantityUnit.Grams, 10);
        var optionalGroup = FoodCustomisationGroup.Create(food.Id, "Sauce", false, 1);
        optionalGroup.AddOption("None", null, 1);
        food.AddCustomisationGroup(optionalGroup);

        _foodRepository
            .FindByExpressionAsync(Arg.Any<FilterSpecification<Domain.Food.Food>>())
            .Returns(food);

        _cartItemRepository
            .FindByExpressionAsync(Arg.Any<FilterSpecification<CartItem>>())
            .Returns((CartItem?)null);

        CartItem? addedItem = null;
        _cartItemRepository.When(r => r.Add(Arg.Any<CartItem>()))
            .Do(ci => addedItem = ci.Arg<CartItem>());

        var request = new AddCartItemRequest
        {
            UserId = userId,
            FoodId = food.Id,
            Quantity = 1,
        };

        await _handler.Handle(request, CancellationToken.None);

        addedItem.Should().NotBeNull();
        addedItem!.Customisations.Should().BeEmpty();
    }

    private static Domain.Food.Food CreateFoodWithRequiredGroup(
        out FoodCustomisationGroup group,
        out FoodCustomisationOption defaultOption)
    {
        var food = Domain.Food.Food.Create("Salmon", "Grilled", 185000m, Guid.NewGuid(), QuantityUnit.Grams, 10);
        group = FoodCustomisationGroup.Create(food.Id, "Portion size", true, 1);
        group.AddOption("Regular (180 g)", null, 1);
        group.AddOption("Large (260 g)", 30000m, 2);
        food.AddCustomisationGroup(group);
        defaultOption = group.Options.OrderBy(o => o.DisplayOrder).First();
        return food;
    }
}
