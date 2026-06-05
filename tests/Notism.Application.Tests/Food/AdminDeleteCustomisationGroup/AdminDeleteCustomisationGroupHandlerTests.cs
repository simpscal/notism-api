using FluentAssertions;

using Microsoft.Extensions.Logging;

using Notism.Application.Common.Services;
using Notism.Application.Food.AdminDeleteCustomisationGroup;
using Notism.Domain.Common.Interfaces;
using Notism.Domain.Common.Specifications;
using Notism.Domain.Food;
using Notism.Domain.Food.Enums;
using Notism.Shared.Exceptions;

using NSubstitute;

namespace Notism.Application.Tests.Food.AdminDeleteCustomisationGroup;

public class AdminDeleteCustomisationGroupHandlerTests
{
    private readonly IRepository<Domain.Food.Food> _foodRepository;
    private readonly ILogger<AdminDeleteCustomisationGroupHandler> _logger;
    private readonly IMessages _messages;
    private readonly AdminDeleteCustomisationGroupHandler _handler;

    public AdminDeleteCustomisationGroupHandlerTests()
    {
        _foodRepository = Substitute.For<IRepository<Domain.Food.Food>>();
        _logger = Substitute.For<ILogger<AdminDeleteCustomisationGroupHandler>>();
        _messages = Substitute.For<IMessages>();

        _messages.FoodNotFound.Returns("Food not found.");
        _messages.CustomisationGroupNotFound.Returns("Customisation group not found.");

        _handler = new AdminDeleteCustomisationGroupHandler(
            _foodRepository,
            _logger,
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

        _foodRepository
            .FindByExpressionAsync(Arg.Any<FilterSpecification<Domain.Food.Food>>())
            .Returns(food);

        var request = new AdminDeleteCustomisationGroupRequest
        {
            FoodId = food.Id,
            GroupId = group.Id,
        };

        await _handler.Handle(request, CancellationToken.None);

        food.CustomisationGroups.Should().BeEmpty();
        await _foodRepository.Received(1).SaveChangesAsync();
    }

    [Fact]
    public async Task Handle_WhenFoodNotFound_ThrowsNotFoundException()
    {
        _foodRepository
            .FindByExpressionAsync(Arg.Any<FilterSpecification<Domain.Food.Food>>())
            .Returns((Domain.Food.Food?)null);

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

        // Group belongs to a different food — not added to this food
        var otherFoodId = Guid.NewGuid();
        var group = FoodCustomisationGroup.Create(otherFoodId, "Toppings", isRequired: false, displayOrder: 1);

        _foodRepository
            .FindByExpressionAsync(Arg.Any<FilterSpecification<Domain.Food.Food>>())
            .Returns(food);

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
