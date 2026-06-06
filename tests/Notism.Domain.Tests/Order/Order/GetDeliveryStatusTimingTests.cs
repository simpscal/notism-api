using FluentAssertions;

using Notism.Domain.Order.Enums;
using Notism.Domain.Payment.Enums;

namespace Notism.Domain.Tests.Order.Order;

public class GetDeliveryStatusTimingTests
{
    [Fact]
    public void GetDeliveryStatusTiming_WhenOnlyOrderPlaced_SetsOrderPlacedCompletedAt()
    {
        var order = Domain.Order.Order.Create(Guid.NewGuid(), PaymentMethod.Banking, new List<Guid>());

        var timing = order.GetDeliveryStatusTiming();

        timing.OrderPlacedCompletedAt.Should().NotBeNull();
        timing.PreparingCompletedAt.Should().BeNull();
        timing.OnTheWayCompletedAt.Should().BeNull();
        timing.DeliveredCompletedAt.Should().BeNull();
    }

    [Fact]
    public void GetDeliveryStatusTiming_WhenPreparing_OverwritesOrderPlacedCompletedAt()
    {
        var order = Domain.Order.Order.Create(Guid.NewGuid(), PaymentMethod.Banking, new List<Guid>());
        order.UpdateDeliveryStatus(DeliveryStatus.Preparing);

        var timing = order.GetDeliveryStatusTiming();

        var preparingAt = order.StatusHistory.First(h => h.Status == DeliveryStatus.Preparing).StatusChangedAt;
        timing.OrderPlacedCompletedAt.Should().Be(preparingAt);
    }

    [Fact]
    public void GetDeliveryStatusTiming_WhenDelivered_SetsAllTransitionTimestamps()
    {
        var order = Domain.Order.Order.Create(Guid.NewGuid(), PaymentMethod.Banking, new List<Guid>());
        order.UpdateDeliveryStatus(DeliveryStatus.Preparing);
        order.UpdateDeliveryStatus(DeliveryStatus.OnTheWay);
        order.UpdateDeliveryStatus(DeliveryStatus.Delivered);

        var timing = order.GetDeliveryStatusTiming();

        var preparingAt = order.StatusHistory.First(h => h.Status == DeliveryStatus.Preparing).StatusChangedAt;
        var onTheWayAt = order.StatusHistory.First(h => h.Status == DeliveryStatus.OnTheWay).StatusChangedAt;
        var deliveredAt = order.StatusHistory.First(h => h.Status == DeliveryStatus.Delivered).StatusChangedAt;

        timing.OrderPlacedCompletedAt.Should().Be(preparingAt);
        timing.PreparingCompletedAt.Should().Be(onTheWayAt);
        timing.OnTheWayCompletedAt.Should().Be(deliveredAt);
        timing.DeliveredCompletedAt.Should().Be(deliveredAt);
    }

    [Fact]
    public void GetDeliveryStatusTiming_WhenDeliveredButCurrentStatusNotDelivered_DoesNotSetDeliveredCompletedAt()
    {
        var order = Domain.Order.Order.Create(Guid.NewGuid(), PaymentMethod.Banking, new List<Guid>());
        order.UpdateDeliveryStatus(DeliveryStatus.Preparing);
        order.UpdateDeliveryStatus(DeliveryStatus.OnTheWay);
        order.UpdateDeliveryStatus(DeliveryStatus.Delivered);
        order.UpdateDeliveryStatus(DeliveryStatus.OrderPlaced);

        var timing = order.GetDeliveryStatusTiming();

        timing.DeliveredCompletedAt.Should().BeNull();
    }
}