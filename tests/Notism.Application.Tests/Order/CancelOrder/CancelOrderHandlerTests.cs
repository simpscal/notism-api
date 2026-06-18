using FluentAssertions;

using Microsoft.Extensions.Logging;

using Notism.Application.Common.Services;
using Notism.Application.Order.CancelOrder;
using Notism.Application.Tests.Common;
using Notism.Domain.Order.Enums;
using Notism.Infrastructure.Repositories;
using Notism.Shared.Exceptions;

using NSubstitute;

using DomainOrder = Notism.Domain.Order.Order;
using PaymentMethodEnum = Notism.Domain.Order.Enums.PaymentMethod;

namespace Notism.Application.Tests.Order.CancelOrder;

public class CancelOrderHandlerTests : IDisposable
{
    private readonly WriteHandlerContext _context;
    private readonly CancelOrderHandler _handler;

    public CancelOrderHandlerTests()
    {
        _context = new WriteHandlerContext();

        var messages = Substitute.For<IMessages>();
        messages.OrderNotFound.Returns("Order not found");

        _handler = new CancelOrderHandler(
            new OrderRepository(_context.DbContext),
            _context.DbContext,
            Substitute.For<ILogger<CancelOrderHandler>>(),
            messages);
    }

    [Fact]
    public async Task Handle_WhenPaidBankingOrder_CancelsAndCreatesPendingFullTotalRefund()
    {
        var order = await SeedOrderAsync(PaymentMethodEnum.Banking, paid: true);

        await _handler.Handle(Request(order), CancellationToken.None);

        var persisted = await ReloadAsync(order.Id);
        persisted.DeliveryStatus.Should().Be(DeliveryStatus.Cancelled);
        persisted.Refund.Should().NotBeNull();
        persisted.Refund!.Status.Should().Be(RefundStatus.Pending);
        persisted.Refund.Amount.Should().Be(order.TotalAmount);
    }

    [Fact]
    public async Task Handle_WhenOrderNotPaid_CancelsWithoutRefund()
    {
        var order = await SeedOrderAsync(PaymentMethodEnum.Banking, paid: false);

        await _handler.Handle(Request(order), CancellationToken.None);

        var persisted = await ReloadAsync(order.Id);
        persisted.DeliveryStatus.Should().Be(DeliveryStatus.Cancelled);
        persisted.Refund.Should().BeNull();
    }

    [Fact]
    public async Task Handle_WhenNotBankingOrder_CancelsWithoutRefund()
    {
        var order = await SeedOrderAsync(PaymentMethodEnum.CashOnDelivery, paid: true);

        await _handler.Handle(Request(order), CancellationToken.None);

        var persisted = await ReloadAsync(order.Id);
        persisted.DeliveryStatus.Should().Be(DeliveryStatus.Cancelled);
        persisted.Refund.Should().BeNull();
    }

    [Fact]
    public async Task Handle_WhenRefundAlreadyExists_DoesNotCreateAnotherRefund()
    {
        var order = await SeedOrderAsync(PaymentMethodEnum.Banking, paid: true, withRefund: true);
        var existingRefundId = order.Refund!.Id;

        await _handler.Handle(Request(order), CancellationToken.None);

        var persisted = await ReloadAsync(order.Id);
        persisted.Refund.Should().NotBeNull();
        persisted.Refund!.Id.Should().Be(existingRefundId);
    }

    [Fact]
    public async Task Handle_WhenAlreadyCancelled_FailsAndLeavesSingleRefund()
    {
        var order = await SeedOrderAsync(PaymentMethodEnum.Banking, paid: true);
        await _handler.Handle(Request(order), CancellationToken.None);

        var secondCancel = () => _handler.Handle(Request(order), CancellationToken.None);

        await secondCancel.Should().ThrowAsync<ResultFailureException>();
        var persisted = await ReloadAsync(order.Id);
        persisted.Refund.Should().NotBeNull();
    }

    [Fact]
    public async Task Handle_WhenOrderNotFound_Throws()
    {
        var act = () => _handler.Handle(
            new CancelOrderRequest { OrderId = Guid.NewGuid(), UserId = Guid.NewGuid() },
            CancellationToken.None);

        await act.Should().ThrowAsync<ResultFailureException>();
    }

    public void Dispose()
        => _context.Dispose();

    private static CancelOrderRequest Request(DomainOrder order)
        => new() { OrderId = order.Id, UserId = order.UserId };

    private async Task<DomainOrder> SeedOrderAsync(PaymentMethodEnum paymentMethod, bool paid, bool withRefund = false)
    {
        var order = DomainOrder.Create(Guid.NewGuid(), paymentMethod, new List<Guid>());
        order.AddItem(Domain.Order.OrderItem.Create(order.Id, Guid.NewGuid(), "Burger", unitPrice: 150_000m, discountPrice: null, quantity: 1));

        if (paid)
        {
            order.MarkAsPaid(DateTime.UtcNow);
        }

        if (withRefund)
        {
            order.CreateRefund();
        }

        order.ClearDomainEvents();
        await _context.SeedAsync(order);
        return order;
    }

    private async Task<DomainOrder> ReloadAsync(Guid orderId)
    {
        _context.DbContext.ChangeTracker.Clear();
        var order = await _context.DbContext.Orders.FindAsync(orderId)
            ?? throw new InvalidOperationException("Order was not persisted.");
        await _context.DbContext.Entry(order).Reference(o => o.Refund).LoadAsync();
        return order;
    }
}