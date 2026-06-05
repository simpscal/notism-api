using FluentAssertions;

using Notism.Domain.Order;

namespace Notism.Domain.Tests.Order.OrderItem;

public class OrderItemTotalPriceTests
{
    [Fact]
    public void Create_WhenNullSurcharge_TotalPriceEqualsEffectivePriceTimesQuantity()
    {
        var unitPrice = 100m;
        var quantity = 3;

        var item = Domain.Order.OrderItem.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Burger",
            unitPrice,
            discountPrice: null,
            quantity,
            surcharge: null,
            customisationLabel: null);

        item.TotalPrice.Should().Be(unitPrice * quantity);
    }

    [Fact]
    public void Create_WhenZeroSurcharge_TotalPriceEqualsEffectivePriceTimesQuantity()
    {
        var unitPrice = 100m;
        var quantity = 2;

        var item = Domain.Order.OrderItem.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Burger",
            unitPrice,
            discountPrice: null,
            quantity,
            surcharge: 0m,
            customisationLabel: null);

        item.TotalPrice.Should().Be(unitPrice * quantity);
    }

    [Fact]
    public void Create_WhenPositiveSurcharge_TotalPriceIncludesSurcharge()
    {
        var unitPrice = 100m;
        var surcharge = 20m;
        var quantity = 2;

        var item = Domain.Order.OrderItem.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Burger",
            unitPrice,
            discountPrice: null,
            quantity,
            surcharge: surcharge,
            customisationLabel: null);

        item.TotalPrice.Should().Be((unitPrice + surcharge) * quantity);
    }

    [Fact]
    public void Create_WhenDiscountPriceAndPositiveSurcharge_TotalPriceUsesDiscountPlusSurcharge()
    {
        var unitPrice = 100m;
        var discountPrice = 80m;
        var surcharge = 15m;
        var quantity = 3;

        var item = Domain.Order.OrderItem.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Burger",
            unitPrice,
            discountPrice,
            quantity,
            surcharge,
            customisationLabel: null);

        item.TotalPrice.Should().Be((discountPrice + surcharge) * quantity);
    }

    [Fact]
    public void Create_WhenCustomisationLabelProvided_StoresLabel()
    {
        var item = Domain.Order.OrderItem.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Burger",
            50m,
            discountPrice: null,
            quantity: 1,
            surcharge: 5m,
            customisationLabel: "Extra Spicy");

        item.CustomisationLabel.Should().Be("Extra Spicy");
        item.Surcharge.Should().Be(5m);
    }

    [Fact]
    public void Create_WhenNoCustomisation_SurchargeAndLabelAreNull()
    {
        var item = Domain.Order.OrderItem.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Burger",
            50m,
            discountPrice: null,
            quantity: 1,
            surcharge: null,
            customisationLabel: null);

        item.Surcharge.Should().BeNull();
        item.CustomisationLabel.Should().BeNull();
    }
}
