using FluentAssertions;

using Notism.Domain.Food.Enums;

namespace Notism.Domain.Tests.Food;

public class PrimaryImageTests
{
    [Fact]
    public void PrimaryImage_WhenNoImages_ReturnsNull()
    {
        var food = CreateFood();

        food.PrimaryImage.Should().BeNull();
    }

    [Fact]
    public void PrimaryImage_WhenMultipleImages_ReturnsImageWithLowestDisplayOrder()
    {
        var food = CreateFood();
        food.AddImage("second.jpg", displayOrder: 2);
        food.AddImage("first.jpg", displayOrder: 1);
        food.AddImage("third.jpg", displayOrder: 3);

        food.PrimaryImage.Should().NotBeNull();
        food.PrimaryImage!.FileKey.Should().Be("first.jpg");
    }

    private static Domain.Food.Food CreateFood() =>
        Domain.Food.Food.Create("Burger", "Tasty", 100m, Guid.NewGuid(), QuantityUnit.Grams, 10);
}