using FluentAssertions;

using Microsoft.Extensions.Logging;

using Notism.Application.Common.Interfaces;
using Notism.Application.Common.Services;
using Notism.Application.Food.GetFoodById;
using Notism.Domain.Common.Interfaces;
using Notism.Domain.Common.Specifications;
using Notism.Domain.Food;
using Notism.Domain.Food.Enums;
using Notism.Shared.Exceptions;

using NSubstitute;

namespace Notism.Application.Tests.Food.GetFoodById;

public class GetFoodByIdHandlerTests
{
    private readonly IRepository<Domain.Food.Food> _foodRepository;
    private readonly IStorageService _storageService;
    private readonly ILogger<GetFoodByIdHandler> _logger;
    private readonly IMessages _messages;
    private readonly GetFoodByIdHandler _handler;

    public GetFoodByIdHandlerTests()
    {
        _foodRepository = Substitute.For<IRepository<Domain.Food.Food>>();
        _storageService = Substitute.For<IStorageService>();
        _logger = Substitute.For<ILogger<GetFoodByIdHandler>>();
        _messages = Substitute.For<IMessages>();

        _messages.FoodNotFound.Returns("Food not found.");

        _handler = new GetFoodByIdHandler(
            _foodRepository,
            _storageService,
            _logger,
            _messages);
    }

    [Fact]
    public async Task Handle_WhenFoodExists_ReturnsResponse()
    {
        var food = Domain.Food.Food.Create(
            "Burger",
            "A delicious burger",
            50000m,
            Guid.NewGuid(),
            QuantityUnit.Grams,
            10);

        _foodRepository
            .FindByExpressionAsync(Arg.Any<FilterSpecification<Domain.Food.Food>>())
            .Returns(food);

        var request = new GetFoodByIdRequest { FoodId = food.Id };
        var result = await _handler.Handle(request, CancellationToken.None);

        result.Should().NotBeNull();
        result.Id.Should().Be(food.Id);
        result.Name.Should().Be("Burger");
    }

    [Fact]
    public async Task Handle_WhenFoodNotFound_ThrowsResultFailureException()
    {
        _foodRepository
            .FindByExpressionAsync(Arg.Any<FilterSpecification<Domain.Food.Food>>())
            .Returns((Domain.Food.Food?)null);

        var request = new GetFoodByIdRequest { FoodId = Guid.NewGuid() };

        var act = async () => await _handler.Handle(request, CancellationToken.None);

        await act.Should().ThrowAsync<ResultFailureException>();
    }

    [Fact]
    public async Task Handle_WhenFoodHasCustomisationGroups_ReturnsCustomisationsInDisplayOrder()
    {
        var food = Domain.Food.Food.Create(
            "Pizza",
            "Delicious pizza",
            100000m,
            Guid.NewGuid(),
            QuantityUnit.Grams,
            5);

        var groupA = FoodCustomisationGroup.Create(food.Id, "Size", isRequired: true, displayOrder: 2);
        var groupB = FoodCustomisationGroup.Create(food.Id, "Toppings", isRequired: false, displayOrder: 1);

        groupA.AddOption("Large", surcharge: 20000m, displayOrder: 1);
        groupB.AddOption("Cheese", surcharge: null, displayOrder: 1);

        food.AddCustomisationGroup(groupA);
        food.AddCustomisationGroup(groupB);

        _foodRepository
            .FindByExpressionAsync(Arg.Any<FilterSpecification<Domain.Food.Food>>())
            .Returns(food);

        var request = new GetFoodByIdRequest { FoodId = food.Id };
        var result = await _handler.Handle(request, CancellationToken.None);

        result.Customisations.Should().HaveCount(2);
        result.Customisations[0].Label.Should().Be("Toppings");
        result.Customisations[1].Label.Should().Be("Size");
    }

    [Fact]
    public async Task Handle_WhenGroupHasZeroOptions_GroupIsFilteredFromResponse()
    {
        var food = Domain.Food.Food.Create(
            "Sandwich",
            "A sandwich",
            40000m,
            Guid.NewGuid(),
            QuantityUnit.Grams,
            8);

        var groupWithOptions = FoodCustomisationGroup.Create(food.Id, "Bread", isRequired: true, displayOrder: 1);
        groupWithOptions.AddOption("White", surcharge: null, displayOrder: 1);

        var groupWithoutOptions = FoodCustomisationGroup.Create(food.Id, "Empty Group", isRequired: false, displayOrder: 2);

        food.AddCustomisationGroup(groupWithOptions);
        food.AddCustomisationGroup(groupWithoutOptions);

        _foodRepository
            .FindByExpressionAsync(Arg.Any<FilterSpecification<Domain.Food.Food>>())
            .Returns(food);

        var request = new GetFoodByIdRequest { FoodId = food.Id };
        var result = await _handler.Handle(request, CancellationToken.None);

        result.Customisations.Should().HaveCount(1);
        result.Customisations[0].Label.Should().Be("Bread");
    }

    [Fact]
    public async Task Handle_WhenOptionHasNonZeroSurcharge_SurchargeIncludedInResponse()
    {
        var food = Domain.Food.Food.Create(
            "Noodles",
            "Yummy noodles",
            60000m,
            Guid.NewGuid(),
            QuantityUnit.Grams,
            3);

        var group = FoodCustomisationGroup.Create(food.Id, "Size", isRequired: false, displayOrder: 1);
        group.AddOption("Large", surcharge: 15000m, displayOrder: 1);
        food.AddCustomisationGroup(group);

        _foodRepository
            .FindByExpressionAsync(Arg.Any<FilterSpecification<Domain.Food.Food>>())
            .Returns(food);

        var request = new GetFoodByIdRequest { FoodId = food.Id };
        var result = await _handler.Handle(request, CancellationToken.None);

        var option = result.Customisations[0].Options[0];
        option.Surcharge.Should().Be(15000m);
    }

    [Fact]
    public async Task Handle_WhenOptionSurchargeIsNullOrZero_SurchargeOmittedFromResponse()
    {
        var food = Domain.Food.Food.Create(
            "Rice",
            "Steamed rice",
            30000m,
            Guid.NewGuid(),
            QuantityUnit.Grams,
            20);

        var group = FoodCustomisationGroup.Create(food.Id, "Portion", isRequired: false, displayOrder: 1);
        group.AddOption("Regular", surcharge: null, displayOrder: 1);
        group.AddOption("Small", surcharge: 0m, displayOrder: 2);
        food.AddCustomisationGroup(group);

        _foodRepository
            .FindByExpressionAsync(Arg.Any<FilterSpecification<Domain.Food.Food>>())
            .Returns(food);

        var request = new GetFoodByIdRequest { FoodId = food.Id };
        var result = await _handler.Handle(request, CancellationToken.None);

        result.Customisations[0].Options[0].Surcharge.Should().BeNull();
        result.Customisations[0].Options[1].Surcharge.Should().BeNull();
    }

    [Fact]
    public async Task Handle_WhenOptionsExist_OptionsReturnedInDisplayOrder()
    {
        var food = Domain.Food.Food.Create(
            "Coffee",
            "Hot coffee",
            25000m,
            Guid.NewGuid(),
            QuantityUnit.Milliliters,
            50);

        var group = FoodCustomisationGroup.Create(food.Id, "Size", isRequired: true, displayOrder: 1);
        group.AddOption("Large", surcharge: 5000m, displayOrder: 3);
        group.AddOption("Small", surcharge: null, displayOrder: 1);
        group.AddOption("Medium", surcharge: 2000m, displayOrder: 2);
        food.AddCustomisationGroup(group);

        _foodRepository
            .FindByExpressionAsync(Arg.Any<FilterSpecification<Domain.Food.Food>>())
            .Returns(food);

        var request = new GetFoodByIdRequest { FoodId = food.Id };
        var result = await _handler.Handle(request, CancellationToken.None);

        var options = result.Customisations[0].Options;
        options[0].Label.Should().Be("Small");
        options[1].Label.Should().Be("Medium");
        options[2].Label.Should().Be("Large");
    }

    [Fact]
    public async Task Handle_WhenFoodHasNoCustomisationGroups_ReturnsEmptyCustomisationsList()
    {
        var food = Domain.Food.Food.Create(
            "Water",
            "Bottled water",
            10000m,
            Guid.NewGuid(),
            QuantityUnit.Grams,
            100);

        _foodRepository
            .FindByExpressionAsync(Arg.Any<FilterSpecification<Domain.Food.Food>>())
            .Returns(food);

        var request = new GetFoodByIdRequest { FoodId = food.Id };
        var result = await _handler.Handle(request, CancellationToken.None);

        result.Customisations.Should().BeEmpty();
    }
}
