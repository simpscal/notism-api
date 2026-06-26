using FluentAssertions;

using Notism.Domain.Order.Events;

using DomainOrder = Notism.Domain.Order.Order;
using PaymentMethodEnum = Notism.Domain.Order.Enums.PaymentMethod;

namespace Notism.Domain.Tests.Order.Order;

public class CreateTests
{
    [Fact]
    public void Create_AssignsNonEmptyIdAtConstruction()
    {
        var order = DomainOrder.Create(Guid.NewGuid(), PaymentMethodEnum.Banking, new List<Guid>());

        order.Id.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public void Create_RaisesOrderCreatedEventCarryingTheOrderId()
    {
        var order = DomainOrder.Create(Guid.NewGuid(), PaymentMethodEnum.Banking, new List<Guid>());

        var createdEvent = order.DomainEvents.OfType<OrderCreatedEvent>().Single();

        createdEvent.OrderId.Should().Be(order.Id);
        createdEvent.OrderId.Should().NotBe(Guid.Empty);
    }
}