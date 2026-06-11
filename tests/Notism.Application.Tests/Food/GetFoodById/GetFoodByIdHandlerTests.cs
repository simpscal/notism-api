using FluentAssertions;

using Microsoft.Extensions.Logging;

using Notism.Application.Common.Services;
using Notism.Application.Food.GetFoodById;
using Notism.Application.Tests.Common;
using Notism.Domain.Food;
using Notism.Domain.Food.Enums;
using Notism.Infrastructure.Persistence;
using Notism.Shared.Exceptions;

using NSubstitute;

namespace Notism.Application.Tests.Food.GetFoodById;

/// <summary>
/// Exercises the <see cref="GetFoodByIdQuery"/> behind <see cref="GetFoodByIdHandler"/>
/// against an EF InMemory database: the by-id/not-deleted predicate, the
/// customisation-group/option graph projection, and the display-order/surcharge mapping.
/// </summary>
public class GetFoodByIdHandlerTests
{
    private readonly AppDbContext _dbContext;
    private readonly IMessages _messages;
    private readonly GetFoodByIdHandler _handler;

    public GetFoodByIdHandlerTests()
    {
        _dbContext = ReadDbContextFactory.Create();
        _messages = Substitute.For<IMessages>();
        _messages.FoodNotFound.Returns("Food not found.");

        _handler = new GetFoodByIdHandler(
            _dbContext,
            Substitute.For<IStorageService>(),
            Substitute.For<ILogger<GetFoodByIdHandler>>(),
            _messages);
    }

    [Fact]
    public async Task Handle_WhenFoodExists_ReturnsResponse()
    {
        var food = CreateFood("Burger");
        await SeedAsync(food);

        var result = await _handler.Handle(new GetFoodByIdRequest { FoodId = food.Id }, CancellationToken.None);

        result.Should().NotBeNull();
        result.Id.Should().Be(food.Id);
        result.Name.Should().Be("Burger");
    }

    [Fact]
    public async Task Handle_WhenFoodNotFound_ThrowsResultFailureException()
    {
        var act = async () => await _handler.Handle(
            new GetFoodByIdRequest { FoodId = Guid.NewGuid() },
            CancellationToken.None);

        await act.Should().ThrowAsync<ResultFailureException>();
    }

    [Fact]
    public async Task Handle_WhenFoodIsDeleted_ThrowsResultFailureException()
    {
        var food = CreateFood("Burger");
        food.MarkAsDeleted();
        await SeedAsync(food);

        var act = async () => await _handler.Handle(
            new GetFoodByIdRequest { FoodId = food.Id },
            CancellationToken.None);

        await act.Should().ThrowAsync<ResultFailureException>();
    }

    [Fact]
    public async Task Handle_WhenFoodHasCustomisationGroups_ReturnsCustomisationsInDisplayOrder()
    {
        var food = CreateFood("Pizza");
        var groupA = FoodCustomisationGroup.Create(food.Id, "Size", isRequired: true, displayOrder: 2);
        var groupB = FoodCustomisationGroup.Create(food.Id, "Toppings", isRequired: false, displayOrder: 1);
        groupA.AddOption("Large", surcharge: 20000m, displayOrder: 1);
        groupB.AddOption("Cheese", surcharge: null, displayOrder: 1);
        food.AddCustomisationGroup(groupA);
        food.AddCustomisationGroup(groupB);
        await SeedAsync(food);

        var result = await _handler.Handle(new GetFoodByIdRequest { FoodId = food.Id }, CancellationToken.None);

        result.Customisations.Should().HaveCount(2);
        result.Customisations[0].Label.Should().Be("Toppings");
        result.Customisations[1].Label.Should().Be("Size");
    }

    [Fact]
    public async Task Handle_WhenGroupHasZeroOptions_GroupIsFilteredFromResponse()
    {
        var food = CreateFood("Sandwich");
        var groupWithOptions = FoodCustomisationGroup.Create(food.Id, "Bread", isRequired: true, displayOrder: 1);
        groupWithOptions.AddOption("White", surcharge: null, displayOrder: 1);
        var groupWithoutOptions = FoodCustomisationGroup.Create(food.Id, "Empty Group", isRequired: false, displayOrder: 2);
        food.AddCustomisationGroup(groupWithOptions);
        food.AddCustomisationGroup(groupWithoutOptions);
        await SeedAsync(food);

        var result = await _handler.Handle(new GetFoodByIdRequest { FoodId = food.Id }, CancellationToken.None);

        result.Customisations.Should().HaveCount(1);
        result.Customisations[0].Label.Should().Be("Bread");
    }

    [Fact]
    public async Task Handle_WhenOptionHasNonZeroSurcharge_SurchargeIncludedInResponse()
    {
        var food = CreateFood("Noodles");
        var group = FoodCustomisationGroup.Create(food.Id, "Size", isRequired: false, displayOrder: 1);
        group.AddOption("Large", surcharge: 15000m, displayOrder: 1);
        food.AddCustomisationGroup(group);
        await SeedAsync(food);

        var result = await _handler.Handle(new GetFoodByIdRequest { FoodId = food.Id }, CancellationToken.None);

        result.Customisations[0].Options[0].Surcharge.Should().Be(15000m);
    }

    [Fact]
    public async Task Handle_WhenOptionSurchargeIsNullOrZero_SurchargeOmittedFromResponse()
    {
        var food = CreateFood("Rice");
        var group = FoodCustomisationGroup.Create(food.Id, "Portion", isRequired: false, displayOrder: 1);
        group.AddOption("Regular", surcharge: null, displayOrder: 1);
        group.AddOption("Small", surcharge: 0m, displayOrder: 2);
        food.AddCustomisationGroup(group);
        await SeedAsync(food);

        var result = await _handler.Handle(new GetFoodByIdRequest { FoodId = food.Id }, CancellationToken.None);

        result.Customisations[0].Options[0].Surcharge.Should().BeNull();
        result.Customisations[0].Options[1].Surcharge.Should().BeNull();
    }

    [Fact]
    public async Task Handle_WhenOptionsExist_OptionsReturnedInDisplayOrder()
    {
        var food = CreateFood("Coffee");
        var group = FoodCustomisationGroup.Create(food.Id, "Size", isRequired: true, displayOrder: 1);
        group.AddOption("Large", surcharge: 5000m, displayOrder: 3);
        group.AddOption("Small", surcharge: null, displayOrder: 1);
        group.AddOption("Medium", surcharge: 2000m, displayOrder: 2);
        food.AddCustomisationGroup(group);
        await SeedAsync(food);

        var result = await _handler.Handle(new GetFoodByIdRequest { FoodId = food.Id }, CancellationToken.None);

        var options = result.Customisations[0].Options;
        options[0].Label.Should().Be("Small");
        options[1].Label.Should().Be("Medium");
        options[2].Label.Should().Be("Large");
    }

    [Fact]
    public async Task Handle_WhenFoodHasNoCustomisationGroups_ReturnsEmptyCustomisationsList()
    {
        var food = CreateFood("Water");
        await SeedAsync(food);

        var result = await _handler.Handle(new GetFoodByIdRequest { FoodId = food.Id }, CancellationToken.None);

        result.Customisations.Should().BeEmpty();
    }

    private static Domain.Food.Food CreateFood(string name)
        => Domain.Food.Food.Create(name, $"{name} description", 50000m, Guid.NewGuid(), QuantityUnit.Grams, 10);

    private async Task SeedAsync(Domain.Food.Food food)
    {
        food.ClearDomainEvents();
        _dbContext.Foods.Add(food);
        await _dbContext.SaveChangesAsync();
        _dbContext.ChangeTracker.Clear();
    }
}
