using FluentAssertions;

using Notism.Domain.Order.Events;

using DomainOrder = Notism.Domain.Order.Order;
using PaymentMethodEnum = Notism.Domain.Order.Enums.PaymentMethod;

namespace Notism.Domain.Tests.Order.Order;

public class CreateTests
{
    [Fact]
    public void Create_RaisesOrderCreatedEventCarryingTheSlugId()
    {
        var order = DomainOrder.Create(Guid.NewGuid(), PaymentMethodEnum.Banking, new List<Guid>());

        var createdEvent = order.DomainEvents.OfType<OrderCreatedEvent>().Single();

        createdEvent.SlugId.Should().Be(order.SlugId);
        createdEvent.SlugId.Should().NotBeNullOrWhiteSpace();
    }
}