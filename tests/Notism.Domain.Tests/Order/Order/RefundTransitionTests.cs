using FluentAssertions;

using Notism.Domain.Order.Enums;
using Notism.Domain.Order.Events;
using Notism.Domain.Payment.Enums;

using DomainOrder = Notism.Domain.Order.Order;
using DomainOrderItem = Notism.Domain.Order.OrderItem;
using PaymentMethodEnum = Notism.Domain.Order.Enums.PaymentMethod;

namespace Notism.Domain.Tests.Order.Order;

public class RefundTransitionTests
{
    [Fact]
    public void CreateRefund_WhenOrderPaidAndBanking_CreatesPendingRefundAndRaisesEvent()
    {
        var order = CreatePaidBankingOrder();

        var refund = order.CreateRefund();

        refund.Status.Should().Be(RefundStatus.Pending);
        refund.Amount.Should().Be(order.TotalAmount);
        order.Refund.Should().BeSameAs(refund);
        order.DomainEvents.Should().ContainSingle(e => e is RefundCreatedEvent);
    }

    [Fact]
    public void CreateRefund_WhenOrderNotPaid_Throws()
    {
        var order = DomainOrder.Create(Guid.NewGuid(), PaymentMethodEnum.Banking, new List<Guid>());

        var act = () => order.CreateRefund();

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void CreateRefund_WhenOrderNotBanking_Throws()
    {
        var order = DomainOrder.Create(Guid.NewGuid(), PaymentMethodEnum.CashOnDelivery, new List<Guid>());
        order.MarkAsPaid(DateTime.UtcNow);

        var act = () => order.CreateRefund();

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void CreateRefund_WhenRefundAlreadyExists_Throws()
    {
        var order = CreatePaidBankingOrder();
        order.CreateRefund();

        var act = () => order.CreateRefund();

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void RequestRefund_WhenNoRefundExists_CreatesPendingRefundAndRaisesEvent()
    {
        var order = CreatePaidBankingOrder();

        var refund = order.RequestRefund();

        refund.Status.Should().Be(RefundStatus.Pending);
        order.Refund.Should().BeSameAs(refund);
        order.DomainEvents.Should().ContainSingle(e => e is RefundCreatedEvent);
    }

    [Fact]
    public void RequestRefund_WhenRefundAlreadyExists_Throws()
    {
        var order = CreateOrderWithPendingRefund();

        var act = () => order.RequestRefund();

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void MarkRefundProcessing_FromPending_TransitionsAndRaisesEvent()
    {
        var order = CreateOrderWithPendingRefund();

        order.MarkRefundProcessing();

        order.Refund!.Status.Should().Be(RefundStatus.Processing);
        order.DomainEvents.Should().ContainSingle(e => e is RefundProcessingEvent);
    }

    [Fact]
    public void MarkRefundProcessing_FromFailed_RetriesToProcessing()
    {
        var order = CreateOrderWithPendingRefund();
        order.MarkRefundFailed("provider declined");
        order.ClearDomainEvents();

        order.MarkRefundProcessing();

        order.Refund!.Status.Should().Be(RefundStatus.Processing);
        order.Refund.FailureReason.Should().BeNull();
        order.DomainEvents.Should().ContainSingle(e => e is RefundProcessingEvent);
    }

    [Fact]
    public void MarkRefundPaid_FromProcessing_TransitionsToPaidAndCapturesReferenceAndRaisesEvent()
    {
        var order = CreateOrderWithPendingRefund();
        order.MarkRefundProcessing();
        order.ClearDomainEvents();

        order.MarkRefundPaid("SEPAY-REF-123");

        order.Refund!.Status.Should().Be(RefundStatus.Paid);
        order.Refund.TransferReference.Should().Be("SEPAY-REF-123");
        order.Refund.PaidAt.Should().NotBeNull();
        order.DomainEvents.Should().ContainSingle(e => e is RefundPaidEvent);
    }

    [Fact]
    public void MarkRefundFailed_FromPending_TransitionsToFailedAndSetsReasonAndRaisesEvent()
    {
        var order = CreateOrderWithPendingRefund();

        order.MarkRefundFailed("insufficient balance");

        order.Refund!.Status.Should().Be(RefundStatus.Failed);
        order.Refund.FailureReason.Should().Be("insufficient balance");
        order.DomainEvents.Should().ContainSingle(e => e is RefundFailedEvent);
    }

    [Fact]
    public void MarkRefundFailed_FromProcessing_TransitionsToFailed()
    {
        var order = CreateOrderWithPendingRefund();
        order.MarkRefundProcessing();
        order.ClearDomainEvents();

        order.MarkRefundFailed("provider timeout");

        order.Refund!.Status.Should().Be(RefundStatus.Failed);
        order.Refund.FailureReason.Should().Be("provider timeout");
        order.DomainEvents.Should().ContainSingle(e => e is RefundFailedEvent);
    }

    [Fact]
    public void MarkRefundPaid_FromPending_Throws()
    {
        var order = CreateOrderWithPendingRefund();

        var act = () => order.MarkRefundPaid("SEPAY-REF-123");

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void MarkRefundProcessing_FromPaid_Throws()
    {
        var order = CreateOrderWithPendingRefund();
        order.MarkRefundProcessing();
        order.MarkRefundPaid("SEPAY-REF-123");

        var act = () => order.MarkRefundProcessing();

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void MarkRefundFailed_FromPaid_Throws()
    {
        var order = CreateOrderWithPendingRefund();
        order.MarkRefundProcessing();
        order.MarkRefundPaid("SEPAY-REF-123");

        var act = () => order.MarkRefundFailed("late failure");

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void MarkRefundPaid_FromFailed_Throws()
    {
        var order = CreateOrderWithPendingRefund();
        order.MarkRefundFailed("provider declined");

        var act = () => order.MarkRefundPaid("SEPAY-REF-123");

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Cancel_WhenOrderPlaced_RaisesOrderCancelledEvent()
    {
        var order = DomainOrder.Create(Guid.NewGuid(), PaymentMethodEnum.Banking, new List<Guid>());

        order.Cancel();

        order.DeliveryStatus.Should().Be(DeliveryStatus.Cancelled);
        order.DomainEvents.Should().ContainSingle(e => e is OrderCancelledEvent);
    }

    private static DomainOrder CreatePaidBankingOrder()
    {
        var order = DomainOrder.Create(Guid.NewGuid(), PaymentMethodEnum.Banking, new List<Guid>());
        order.AddItem(DomainOrderItem.Create(order.Id, Guid.NewGuid(), "Burger", unitPrice: 50m, discountPrice: null, quantity: 2));
        order.MarkAsPaid(DateTime.UtcNow);
        order.ClearDomainEvents();
        return order;
    }

    private static DomainOrder CreateOrderWithPendingRefund()
    {
        var order = CreatePaidBankingOrder();
        order.RequestRefund();
        order.ClearDomainEvents();
        return order;
    }
}