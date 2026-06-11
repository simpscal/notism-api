using FluentAssertions;

using Microsoft.Extensions.Logging;

using Notism.Application.Common.Services;
using Notism.Application.Food.GetFoods;
using Notism.Application.Tests.Common;
using Notism.Domain.Food;
using Notism.Domain.Food.Enums;
using Notism.Infrastructure.Persistence;

using NSubstitute;

namespace Notism.Application.Tests.Food.GetFoods;

/// <summary>
/// Exercises the <see cref="GetFoodsQuery"/> behind <see cref="GetFoodsHandler"/> against
/// an EF InMemory database: the not-deleted / category / availability / keyword filters,
/// the projection shape, and the keyword-lowercasing contract.
/// </summary>
public class GetFoodsHandlerTests
{
    private readonly AppDbContext _dbContext;
    private readonly IStorageService _storageService;
    private readonly GetFoodsHandler _handler;

    public GetFoodsHandlerTests()
    {
        _dbContext = ReadDbContextFactory.Create();
        _storageService = Substitute.For<IStorageService>();
        _storageService
            .GetPublicUrl(Arg.Any<string>(), Arg.Any<string>())
            .Returns(call => $"https://cdn.test/{call.ArgAt<string>(0)}");

        _handler = new GetFoodsHandler(
            _dbContext,
            _storageService,
            Substitute.For<ILogger<GetFoodsHandler>>());
    }

    [Fact]
    public async Task Handle_WhenKeywordMatchesName_ReturnsFood()
    {
        await SeedAsync(CreateFood(name: "Burger Deluxe", description: "no match here"));

        var result = await _handler.Handle(new GetFoodsRequest { Keyword = "BUR" }, CancellationToken.None);

        result.Items.Should().ContainSingle(i => i.Name == "Burger Deluxe");
    }

    [Fact]
    public async Task Handle_WhenKeywordMatchesDescription_ReturnsFood()
    {
        await SeedAsync(CreateFood(name: "Wings", description: "Extra SPICY chicken wings"));

        var result = await _handler.Handle(new GetFoodsRequest { Keyword = "spicy" }, CancellationToken.None);

        result.Items.Should().ContainSingle(i => i.Name == "Wings");
    }

    [Fact]
    public async Task Handle_WhenKeywordMatchesNeitherNameNorDescription_ExcludesFood()
    {
        await SeedAsync(CreateFood(name: "Burger", description: "A beef sandwich"));

        var result = await _handler.Handle(new GetFoodsRequest { Keyword = "pizza" }, CancellationToken.None);

        result.Items.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_WhenNoKeyword_ReturnsAllNonDeletedFoods()
    {
        await SeedAsync(CreateFood(name: "Anything", description: "Whatever"));

        var result = await _handler.Handle(new GetFoodsRequest(), CancellationToken.None);

        result.Items.Should().ContainSingle(i => i.Name == "Anything");
    }

    [Fact]
    public async Task Handle_WhenCategoryFilterProvided_ReturnsOnlyMatchingCategory()
    {
        await SeedAsync(
            CreateFood(name: "Cola", categoryName: "Drinks"),
            CreateFood(name: "Steak", categoryName: "Mains"));

        var result = await _handler.Handle(new GetFoodsRequest { Category = "Drinks" }, CancellationToken.None);

        result.Items.Should().ContainSingle(i => i.Name == "Cola");
    }

    [Fact]
    public async Task Handle_WhenIsAvailableFilterTrue_ReturnsOnlyAvailableFoods()
    {
        await SeedAsync(
            CreateFood(name: "Available", isAvailable: true),
            CreateFood(name: "Unavailable", isAvailable: false));

        var result = await _handler.Handle(new GetFoodsRequest { IsAvailable = true }, CancellationToken.None);

        result.Items.Should().ContainSingle(i => i.Name == "Available");
    }

    [Fact]
    public async Task Handle_WhenIsAvailableFilterFalse_ReturnsOnlyUnavailableFoods()
    {
        await SeedAsync(
            CreateFood(name: "Available", isAvailable: true),
            CreateFood(name: "Unavailable", isAvailable: false));

        var result = await _handler.Handle(new GetFoodsRequest { IsAvailable = false }, CancellationToken.None);

        result.Items.Should().ContainSingle(i => i.Name == "Unavailable");
    }

    [Fact]
    public async Task Handle_WhenFoodIsDeleted_ExcludesFood()
    {
        var deleted = CreateFood(name: "Deleted");
        deleted.MarkAsDeleted();
        await SeedAsync(deleted);

        var result = await _handler.Handle(new GetFoodsRequest(), CancellationToken.None);

        result.Items.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_WhenFoodHasImage_MapsProjectionToResponse()
    {
        var food = CreateFood(name: "Burger", description: "A delicious burger", categoryName: "Mains");
        food.AddImage("foods/burger.jpg", 0, null);
        await SeedAsync(food);

        var result = await _handler.Handle(new GetFoodsRequest { Keyword = "burger" }, CancellationToken.None);

        result.TotalCount.Should().Be(1);
        var item = result.Items.Single();
        item.Name.Should().Be("Burger");
        item.Description.Should().Be("A delicious burger");
        item.Category.Should().Be("Mains");
        item.ImageUrl.Should().Be("https://cdn.test/foods/burger.jpg");
    }

    [Fact]
    public async Task Handle_WhenProjectionHasNoImages_ImageUrlIsEmpty()
    {
        await SeedAsync(CreateFood(name: "Water", categoryName: "Drinks"));

        var result = await _handler.Handle(new GetFoodsRequest(), CancellationToken.None);

        result.Items.Single().ImageUrl.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_WhenKeywordProvided_KeywordIsLowercasedBeforeMatching()
    {
        await SeedAsync(CreateFood(name: "Double Burger", description: "no match"));

        var result = await _handler.Handle(new GetFoodsRequest { Keyword = "BURGER" }, CancellationToken.None);

        result.Items.Should().ContainSingle(i => i.Name == "Double Burger");
    }

    private async Task SeedAsync(params Domain.Food.Food[] foods)
    {
        foreach (var food in foods)
        {
            food.ClearDomainEvents();
            _dbContext.Foods.Add(food);
        }

        await _dbContext.SaveChangesAsync();
        _dbContext.ChangeTracker.Clear();
    }

    private Domain.Food.Food CreateFood(
        string name = "Burger",
        string description = "A delicious burger",
        string? categoryName = null,
        bool isAvailable = true)
    {
        var category = Category.Create(categoryName ?? $"Category-{Guid.NewGuid():N}");
        _dbContext.Categories.Add(category);

        var food = Domain.Food.Food.Create(
            name,
            description,
            50000m,
            category.Id,
            QuantityUnit.Grams,
            10);

        if (!isAvailable)
        {
            food.SetAvailability(false);
        }

        return food;
    }
}
