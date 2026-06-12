using FluentAssertions;

using Notism.Application.Food.GetAvailableFoodCount;
using Notism.Application.Tests.Common;
using Notism.Domain.Food;
using Notism.Domain.Food.Enums;
using Notism.Infrastructure.Persistence;

namespace Notism.Application.Tests.Food.GetAvailableFoodCount;

public class GetAvailableFoodCountHandlerTests
{
    private readonly AppDbContext _dbContext;
    private readonly GetAvailableFoodCountHandler _handler;

    public GetAvailableFoodCountHandlerTests()
    {
        _dbContext = ReadDbContextFactory.Create();
        _handler = new GetAvailableFoodCountHandler(_dbContext);
    }

    [Fact]
    public async Task Handle_CountsOnlyAvailableNonDeletedFoods()
    {
        await SeedAsync(
            CreateFood("Available", isAvailable: true, isDeleted: false),
            CreateFood("Unavailable", isAvailable: false, isDeleted: false),
            CreateFood("Deleted", isAvailable: true, isDeleted: true));

        var result = await _handler.Handle(new GetAvailableFoodCountRequest(), CancellationToken.None);

        result.Count.Should().Be(1);
    }

    [Fact]
    public async Task Handle_WhenCategoryProvided_CountsOnlyThatCategory()
    {
        await SeedAsync(
            CreateFood("Cola", isAvailable: true, isDeleted: false, categoryName: "Drinks"),
            CreateFood("Steak", isAvailable: true, isDeleted: false, categoryName: "Mains"));

        var result = await _handler.Handle(
            new GetAvailableFoodCountRequest { Category = "Drinks" },
            CancellationToken.None);

        result.Count.Should().Be(1);
    }

    private Domain.Food.Food CreateFood(string name, bool isAvailable, bool isDeleted, string? categoryName = null)
    {
        var category = Category.Create(categoryName ?? $"Category-{Guid.NewGuid():N}");
        _dbContext.Categories.Add(category);

        var food = Domain.Food.Food.Create(name, $"{name} description", 50000m, category.Id, QuantityUnit.Grams, 10);

        if (!isAvailable)
        {
            food.SetAvailability(false);
        }

        if (isDeleted)
        {
            food.MarkAsDeleted();
        }

        return food;
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
}