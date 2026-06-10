using FluentAssertions;

using Microsoft.Extensions.Logging;

using Notism.Application.Common.Services;
using Notism.Application.Food.AdminAddCustomisationGroup;
using Notism.Domain.Common.Repositories;
using Notism.Domain.Common.Specifications;
using Notism.Domain.Food;
using Notism.Domain.Food.Enums;
using Notism.Shared.Exceptions;

using NSubstitute;

namespace Notism.Application.Tests.Food.AdminAddCustomisationGroup;

public class AdminAddCustomisationGroupHandlerTests
{
    private readonly IRepository<Domain.Food.Food> _foodRepository;
    private readonly ILogger<AdminAddCustomisationGroupHandler> _logger;
    private readonly IMessages _messages;
    private readonly AdminAddCustomisationGroupHandler _handler;

    public AdminAddCustomisationGroupHandlerTests()
    {
        _foodRepository = Substitute.For<IRepository<Domain.Food.Food>>();
        _logger = Substitute.For<ILogger<AdminAddCustomisationGroupHandler>>();
        _messages = Substitute.For<IMessages>();

        _messages.FoodNotFound.Returns("Food not found.");

        _handler = new AdminAddCustomisationGroupHandler(
            _foodRepository,
            _logger,
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

        _foodRepository
            .FindByExpressionAsync(Arg.Any<ISpecification<Domain.Food.Food>>())
            .Returns(food);

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

        await _foodRepository.Received(1).SaveChangesAsync();
    }

    [Fact]
    public async Task Handle_WhenFoodNotFound_ThrowsNotFoundException()
    {
        _foodRepository
            .FindByExpressionAsync(Arg.Any<ISpecification<Domain.Food.Food>>())
            .Returns((Domain.Food.Food?)null);

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
