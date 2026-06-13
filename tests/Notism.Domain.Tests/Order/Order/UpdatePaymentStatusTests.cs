using FluentAssertions;

using Notism.Domain.Payment.Enums;

using DomainOrder = Notism.Domain.Order.Order;
using PaymentMethodEnum = Notism.Domain.Order.Enums.PaymentMethod;

namespace Notism.Domain.Tests.Order.Order;

public class UpdatePaymentStatusTests
{
    [Fact]
    public void UpdatePaymentStatus_WhenTargetIsPaid_MarksOrderAsPaidAndSetsPaidAt()
    {
        var order = DomainOrder.Create(Guid.NewGuid(), PaymentMethodEnum.Banking, new List<Guid>());

        order.UpdatePaymentStatus(PaymentStatus.Paid);

        order.PaymentStatus.Should().Be(PaymentStatus.Paid);
        order.PaidAt.Should().NotBeNull();
    }

    [Fact]
    public void UpdatePaymentStatus_WhenTargetIsFailed_MarksOrderAsFailed()
    {
        var order = DomainOrder.Create(Guid.NewGuid(), PaymentMethodEnum.Banking, new List<Guid>());

        order.UpdatePaymentStatus(PaymentStatus.Failed);

        order.PaymentStatus.Should().Be(PaymentStatus.Failed);
        order.PaidAt.Should().BeNull();
    }

    [Fact]
    public void UpdatePaymentStatus_WhenTargetIsUnpaidAfterPaid_RevertsToUnpaidAndClearsPaidAt()
    {
        var order = DomainOrder.Create(Guid.NewGuid(), PaymentMethodEnum.Banking, new List<Guid>());
        order.UpdatePaymentStatus(PaymentStatus.Paid);

        order.UpdatePaymentStatus(PaymentStatus.Unpaid);

        order.PaymentStatus.Should().Be(PaymentStatus.Unpaid);
        order.PaidAt.Should().BeNull();
    }

    [Fact]
    public void UpdatePaymentStatus_WhenTargetEqualsCurrent_IsNoOp()
    {
        var order = DomainOrder.Create(Guid.NewGuid(), PaymentMethodEnum.Banking, new List<Guid>());
        var originalUpdatedAt = order.UpdatedAt;
        order.ClearDomainEvents();

        order.UpdatePaymentStatus(PaymentStatus.Unpaid);

        order.PaymentStatus.Should().Be(PaymentStatus.Unpaid);
        order.PaidAt.Should().BeNull();
        order.UpdatedAt.Should().Be(originalUpdatedAt);
        order.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public void UpdatePaymentStatus_WhenTargetIsRefundedAfterPaid_MarksRefundedAndKeepsPaidAt()
    {
        var order = DomainOrder.Create(Guid.NewGuid(), PaymentMethodEnum.Banking, new List<Guid>());
        order.UpdatePaymentStatus(PaymentStatus.Paid);
        var paidAt = order.PaidAt;

        order.UpdatePaymentStatus(PaymentStatus.Refunded);

        order.PaymentStatus.Should().Be(PaymentStatus.Refunded);
        order.PaidAt.Should().Be(paidAt);
    }

    [Fact]
    public void MarkAsRefunded_WhenCalled_SetsStatusRefundedAndKeepsPaidAt()
    {
        var order = DomainOrder.Create(Guid.NewGuid(), PaymentMethodEnum.Banking, new List<Guid>());
        order.MarkAsPaid(DateTime.UtcNow);
        var paidAt = order.PaidAt;
        order.ClearDomainEvents();

        order.MarkAsRefunded();

        order.PaymentStatus.Should().Be(PaymentStatus.Refunded);
        order.PaidAt.Should().Be(paidAt);
        order.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public void UpdatePaymentStatus_WhenChangedRegardlessOfPaymentMethod_AllowsTransitionForCashOnDelivery()
    {
        var order = DomainOrder.Create(Guid.NewGuid(), PaymentMethodEnum.CashOnDelivery, new List<Guid>());

        order.UpdatePaymentStatus(PaymentStatus.Paid);

        order.PaymentStatus.Should().Be(PaymentStatus.Paid);
        order.PaidAt.Should().NotBeNull();
    }

    [Fact]
    public void RevertToUnpaid_WhenCalled_SetsStatusUnpaidAndClearsPaidAt()
    {
        var order = DomainOrder.Create(Guid.NewGuid(), PaymentMethodEnum.Banking, new List<Guid>());
        order.MarkAsPaid(DateTime.UtcNow);

        order.RevertToUnpaid();

        order.PaymentStatus.Should().Be(PaymentStatus.Unpaid);
        order.PaidAt.Should().BeNull();
    }
}