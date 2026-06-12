using FluentAssertions;

using Microsoft.Extensions.Logging;

using Notism.Application.Common.Services;
using Notism.Application.Food.AdminAddCustomisationGroup;
using Notism.Application.Tests.Common;
using Notism.Domain.Food.Enums;
using Notism.Shared.Exceptions;

using NSubstitute;

namespace Notism.Application.Tests.Food.AdminAddCustomisationGroup;

public class AdminAddCustomisationGroupHandlerTests
{
    private readonly WriteHandlerContext _context;
    private readonly IMessages _messages;
    private readonly AdminAddCustomisationGroupHandler _handler;

    public AdminAddCustomisationGroupHandlerTests()
    {
        _context = new WriteHandlerContext();
        _messages = Substitute.For<IMessages>();

        _messages.FoodNotFound.Returns("Food not found.");

        _handler = new AdminAddCustomisationGroupHandler(
            _context.RepositoryFor<Domain.Food.Food>(),
            _context.DbContext,
            Substitute.For<ILogger<AdminAddCustomisationGroupHandler>>(),
            _messages);
    }

    [Fact]
    public async Task Handle_WhenFoodExists_ReturnsCreatedGroup()
    {
        var food = Domain.Food.Food.Create(
            "Pizza",
            "Delicious pizza",
            100000m,
            Guid.NewGuid(),
            QuantityUnit.Grams,
            5);
        await _context.SeedAsync(food);

        var request = new AdminAddCustomisationGroupRequest
        {
            FoodId = food.Id,
            Label = "Size",
            IsRequired = true,
            DisplayOrder = 1,
        };

        var result = await _handler.Handle(request, CancellationToken.None);

        result.Should().NotBeNull();
        result.FoodId.Should().Be(food.Id);
        result.Label.Should().Be("Size");
        result.IsRequired.Should().BeTrue();
        result.DisplayOrder.Should().Be(1);

        _context.DbContext.ChangeTracker.Clear();
        var persisted = _context.DbContext.FoodCustomisationGroups
            .Where(g => g.FoodId == food.Id)
            .ToList();
        persisted.Should().ContainSingle(g => g.Label == "Size");
    }

    [Fact]
    public async Task Handle_WhenFoodNotFound_ThrowsNotFoundException()
    {
        var request = new AdminAddCustomisationGroupRequest
        {
            FoodId = Guid.NewGuid(),
            Label = "Size",
            IsRequired = false,
            DisplayOrder = 0,
        };

        var act = async () => await _handler.Handle(request, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }
}
