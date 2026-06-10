using System.Linq.Expressions;

using FluentAssertions;

using Microsoft.Extensions.Logging;

using Notism.Application.Common.Services;
using Notism.Application.Food.GetFoods;
using Notism.Domain.Common.Specifications;
using Notism.Domain.Food;
using Notism.Domain.Food.Enums;
using Notism.Domain.Food.Repositories;
using Notism.Shared.Models;

using NSubstitute;

namespace Notism.Application.Tests.Food.GetFoods;

public class GetFoodsHandlerTests
{
    private readonly IFoodRepository _foodRepository;
    private readonly IStorageService _storageService;
    private readonly ILogger<GetFoodsHandler> _logger;
    private readonly GetFoodsHandler _handler;

    public GetFoodsHandlerTests()
    {
        _foodRepository = Substitute.For<IFoodRepository>();
        _storageService = Substitute.For<IStorageService>();
        _logger = Substitute.For<ILogger<GetFoodsHandler>>();

        _storageService
            .GetPublicUrl(Arg.Any<string>(), Arg.Any<string>())
            .Returns(call => $"https://cdn.test/{call.ArgAt<string>(0)}");

        _handler = new GetFoodsHandler(
            _foodRepository,
            _storageService,
            _logger);
    }

    [Fact]
    public async Task Handle_WhenKeywordMatchesName_FoodSatisfiesSpecification()
    {
        var request = new GetFoodsRequest { Keyword = "BUR" };

        var specification = await CaptureSpecificationAsync(request);

        var matching = CreateFood(name: "Burger Deluxe", description: "no match here");
        specification.IsSatisfiedBy(matching).Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WhenKeywordMatchesDescription_FoodSatisfiesSpecification()
    {
        var request = new GetFoodsRequest { Keyword = "spicy" };

        var specification = await CaptureSpecificationAsync(request);

        var matching = CreateFood(name: "Wings", description: "Extra SPICY chicken wings");
        specification.IsSatisfiedBy(matching).Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WhenKeywordMatchesNeitherNameNorDescription_FoodDoesNotSatisfySpecification()
    {
        var request = new GetFoodsRequest { Keyword = "pizza" };

        var specification = await CaptureSpecificationAsync(request);

        var nonMatching = CreateFood(name: "Burger", description: "A beef sandwich");
        specification.IsSatisfiedBy(nonMatching).Should().BeFalse();
    }

    [Fact]
    public async Task Handle_WhenNoKeyword_AllNonDeletedFoodsSatisfySpecification()
    {
        var request = new GetFoodsRequest();

        var specification = await CaptureSpecificationAsync(request);

        var anyFood = CreateFood(name: "Anything", description: "Whatever");
        specification.IsSatisfiedBy(anyFood).Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WhenCategoryFilterProvided_OnlyMatchingCategorySatisfiesSpecification()
    {
        var request = new GetFoodsRequest { Category = "Drinks" };

        var specification = await CaptureSpecificationAsync(request);

        var matching = CreateFood(categoryName: "Drinks");
        var nonMatching = CreateFood(categoryName: "Mains");

        specification.IsSatisfiedBy(matching).Should().BeTrue();
        specification.IsSatisfiedBy(nonMatching).Should().BeFalse();
    }

    [Fact]
    public async Task Handle_WhenIsAvailableFilterTrue_OnlyAvailableFoodsSatisfySpecification()
    {
        var request = new GetFoodsRequest { IsAvailable = true };

        var specification = await CaptureSpecificationAsync(request);

        var available = CreateFood(isAvailable: true);
        var unavailable = CreateFood(isAvailable: false);

        specification.IsSatisfiedBy(available).Should().BeTrue();
        specification.IsSatisfiedBy(unavailable).Should().BeFalse();
    }

    [Fact]
    public async Task Handle_WhenIsAvailableFilterFalse_OnlyUnavailableFoodsSatisfySpecification()
    {
        var request = new GetFoodsRequest { IsAvailable = false };

        var specification = await CaptureSpecificationAsync(request);

        var available = CreateFood(isAvailable: true);
        var unavailable = CreateFood(isAvailable: false);

        specification.IsSatisfiedBy(available).Should().BeFalse();
        specification.IsSatisfiedBy(unavailable).Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WhenFoodIsDeleted_FoodDoesNotSatisfySpecification()
    {
        var request = new GetFoodsRequest();

        var specification = await CaptureSpecificationAsync(request);

        var deleted = CreateFood();
        deleted.MarkAsDeleted();

        specification.IsSatisfiedBy(deleted).Should().BeFalse();
    }

    [Fact]
    public async Task Handle_WhenRepositoryReturnsProjections_MapsToResponseItems()
    {
        SetupRepositoryReturning(new FoodListProjection
        {
            Id = Guid.NewGuid(),
            Name = "Burger",
            Description = "A delicious burger",
            Price = 50000m,
            DiscountPrice = 40000m,
            CategoryName = "Mains",
            IsAvailable = true,
            QuantityUnit = QuantityUnit.Grams,
            StockQuantity = 10,
            ImageKeys = new List<ImageKeyOrder> { new("foods/burger.jpg", 0) },
        });

        var result = await _handler.Handle(new GetFoodsRequest { Keyword = "burger" }, CancellationToken.None);

        result.TotalCount.Should().Be(1);
        result.Items.Should().HaveCount(1);

        var item = result.Items.Single();
        item.Name.Should().Be("Burger");
        item.Description.Should().Be("A delicious burger");
        item.Category.Should().Be("Mains");
        item.Price.Should().Be(50000m);
        item.DiscountPrice.Should().Be(40000m);
        item.ImageUrl.Should().Be("https://cdn.test/foods/burger.jpg");
    }

    [Fact]
    public async Task Handle_WhenProjectionHasNoImages_ImageUrlIsEmpty()
    {
        SetupRepositoryReturning(new FoodListProjection
        {
            Id = Guid.NewGuid(),
            Name = "Water",
            Description = "Bottled water",
            Price = 10000m,
            DiscountPrice = null,
            CategoryName = "Drinks",
            IsAvailable = true,
            QuantityUnit = QuantityUnit.Milliliters,
            StockQuantity = 100,
            ImageKeys = new List<ImageKeyOrder>(),
        });

        var result = await _handler.Handle(new GetFoodsRequest(), CancellationToken.None);

        result.Items.Single().ImageUrl.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_WhenKeywordProvided_KeywordIsLowercasedBeforeMatching()
    {
        // Handler lowercases the keyword; spec compares against col.ToLower().
        var request = new GetFoodsRequest { Keyword = "BURGER" };

        var specification = await CaptureSpecificationAsync(request);

        var matching = CreateFood(name: "Double Burger", description: "no match");
        specification.IsSatisfiedBy(matching).Should().BeTrue();
    }

    private static Domain.Food.Food CreateFood(
        string name = "Burger",
        string description = "A delicious burger",
        string? categoryName = null,
        bool isAvailable = true)
    {
        var food = Domain.Food.Food.Create(
            name,
            description,
            50000m,
            Guid.NewGuid(),
            QuantityUnit.Grams,
            10);

        if (categoryName is not null)
        {
            var category = Category.Create(categoryName);
            typeof(Domain.Food.Food)
                .GetProperty(nameof(Domain.Food.Food.Category))!
                .SetValue(food, category);
        }

        if (!isAvailable)
        {
            food.SetAvailability(false);
        }

        return food;
    }

    private void SetupRepositoryReturning(params FoodListProjection[] projections)
    {
        _foodRepository
            .FilterPagedByExpressionAsync(
                Arg.Any<ISpecification<Domain.Food.Food>>(),
                Arg.Any<Pagination>(),
                Arg.Any<Expression<Func<Domain.Food.Food, FoodListProjection>>>())
            .Returns(new PagedResult<FoodListProjection>
            {
                TotalCount = projections.Length,
                Items = projections,
            });
    }

    private async Task<ISpecification<Domain.Food.Food>> CaptureSpecificationAsync(GetFoodsRequest request)
    {
        ISpecification<Domain.Food.Food>? captured = null;

        _foodRepository
            .FilterPagedByExpressionAsync(
                Arg.Do<ISpecification<Domain.Food.Food>>(spec => captured = spec),
                Arg.Any<Pagination>(),
                Arg.Any<Expression<Func<Domain.Food.Food, FoodListProjection>>>())
            .Returns(new PagedResult<FoodListProjection>
            {
                TotalCount = 0,
                Items = Array.Empty<FoodListProjection>(),
            });

        await _handler.Handle(request, CancellationToken.None);

        captured.Should().NotBeNull();
        return captured!;
    }
}
