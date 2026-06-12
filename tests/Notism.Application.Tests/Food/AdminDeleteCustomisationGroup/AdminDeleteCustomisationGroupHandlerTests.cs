using FluentAssertions;

using Microsoft.Extensions.Logging;

using Notism.Application.Common.Services;
using Notism.Application.Food.AdminDeleteCustomisationGroup;
using Notism.Application.Tests.Common;
using Notism.Domain.Food;
using Notism.Domain.Food.Enums;
using Notism.Shared.Exceptions;

using NSubstitute;

namespace Notism.Application.Tests.Food.AdminDeleteCustomisationGroup;

public class AdminDeleteCustomisationGroupHandlerTests
{
    private readonly WriteHandlerContext _context;
    private readonly IMessages _messages;
    private readonly AdminDeleteCustomisationGroupHandler _handler;

    public AdminDeleteCustomisationGroupHandlerTests()
    {
        _context = new WriteHandlerContext();
        _messages = Substitute.For<IMessages>();

        _messages.FoodNotFound.Returns("Food not found.");
        _messages.CustomisationGroupNotFound.Returns("Customisation group not found.");

        _handler = new AdminDeleteCustomisationGroupHandler(
            _context.RepositoryFor<Domain.Food.Food>(),
            _context.DbContext,
            Substitute.For<ILogger<AdminDeleteCustomisationGroupHandler>>(),
            _messages);
    }

    [Fact]
    public async Task Handle_WhenFoodAndGroupExist_RemovesGroupAndSaves()
    {
        var food = Domain.Food.Food.Create(
            "Burger",
            "A delicious burger",
            50000m,
            Guid.NewGuid(),
            QuantityUnit.Grams,
            10);

        var group = FoodCustomisationGroup.Create(food.Id, "Size", isRequired: true, displayOrder: 1);
        food.AddCustomisationGroup(group);
        await _context.SeedAsync(food);

        var request = new AdminDeleteCustomisationGroupRequest
        {
            FoodId = food.Id,
            GroupId = group.Id,
        };

        await _handler.Handle(request, CancellationToken.None);

        _context.DbContext.ChangeTracker.Clear();
        _context.DbContext.FoodCustomisationGroups
            .Where(g => g.FoodId == food.Id)
            .Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_WhenFoodNotFound_ThrowsNotFoundException()
    {
        var request = new AdminDeleteCustomisationGroupRequest
        {
            FoodId = Guid.NewGuid(),
            GroupId = Guid.NewGuid(),
        };

        var act = async () => await _handler.Handle(request, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("Food not found.");
    }

    [Fact]
    public async Task Handle_WhenGroupBelongsToDifferentFood_ThrowsNotFoundException()
    {
        var food = Domain.Food.Food.Create(
            "Pizza",
            "Tasty pizza",
            80000m,
            Guid.NewGuid(),
            QuantityUnit.Grams,
            5);
        await _context.SeedAsync(food);

        // Group belongs to a different food — not added to this food
        var group = FoodCustomisationGroup.Create(Guid.NewGuid(), "Toppings", isRequired: false, displayOrder: 1);

        var request = new AdminDeleteCustomisationGroupRequest
        {
            FoodId = food.Id,
            GroupId = group.Id,
        };

        var act = async () => await _handler.Handle(request, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("Customisation group not found.");
    }
}
