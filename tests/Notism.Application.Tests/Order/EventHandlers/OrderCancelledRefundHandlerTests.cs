using FluentAssertions;

using Microsoft.Extensions.Logging;

using Notism.Application.Order.EventHandlers;
using Notism.Application.Tests.Common;
using Notism.Domain.Order.Enums;
using Notism.Domain.Order.Events;
using Notism.Infrastructure.Repositories;

using NSubstitute;

using DomainOrder = Notism.Domain.Order.Order;
using PaymentMethodEnum = Notism.Domain.Order.Enums.PaymentMethod;

namespace Notism.Application.Tests.Order.EventHandlers;

public class OrderCancelledRefundHandlerTests : IDisposable
{
    private readonly WriteHandlerContext _context;
    private readonly OrderCancelledRefundHandler _handler;

    public OrderCancelledRefundHandlerTests()
    {
        _context = new WriteHandlerContext();
        _handler = new OrderCancelledRefundHandler(
            new OrderRepository(_context.DbContext),
            _context.DbContext,
            Substitute.For<ILogger<OrderCancelledRefundHandler>>());
    }

    [Fact]
    public async Task Handle_WhenPaidBankingOrder_CreatesPendingFullTotalRefund()
    {
        var order = await SeedOrderAsync(PaymentMethodEnum.Banking, paid: true);

        await _handler.Handle(new OrderCancelledEvent(order.Id, order.UserId), CancellationToken.None);

        var refund = await GetPersistedRefundAsync(order.Id);
        refund.Should().NotBeNull();
        refund!.Status.Should().Be(RefundStatus.Pending);
        refund.Amount.Should().Be(order.TotalAmount);
    }

    [Fact]
    public async Task Handle_WhenOrderNotPaid_DoesNotCreateRefund()
    {
        var order = await SeedOrderAsync(PaymentMethodEnum.Banking, paid: false);

        await _handler.Handle(new OrderCancelledEvent(order.Id, order.UserId), CancellationToken.None);

        var refund = await GetPersistedRefundAsync(order.Id);
        refund.Should().BeNull();
    }

    [Fact]
    public async Task Handle_WhenNotBankingOrder_DoesNotCreateRefund()
    {
        var order = await SeedOrderAsync(PaymentMethodEnum.CashOnDelivery, paid: true);

        await _handler.Handle(new OrderCancelledEvent(order.Id, order.UserId), CancellationToken.None);

        var refund = await GetPersistedRefundAsync(order.Id);
        refund.Should().BeNull();
    }

    [Fact]
    public async Task Handle_WhenRefundAlreadyExists_DoesNotCreateAnotherRefund()
    {
        var order = await SeedOrderAsync(PaymentMethodEnum.Banking, paid: true, withRefund: true);
        var existingRefundId = order.Refund!.Id;

        await _handler.Handle(new OrderCancelledEvent(order.Id, order.UserId), CancellationToken.None);

        var refund = await GetPersistedRefundAsync(order.Id);
        refund.Should().NotBeNull();
        refund!.Id.Should().Be(existingRefundId);
    }

    [Fact]
    public async Task Handle_WhenInvokedTwice_IsIdempotent()
    {
        var order = await SeedOrderAsync(PaymentMethodEnum.Banking, paid: true);

        await _handler.Handle(new OrderCancelledEvent(order.Id, order.UserId), CancellationToken.None);
        var firstRefundId = (await GetPersistedRefundAsync(order.Id))!.Id;

        await _handler.Handle(new OrderCancelledEvent(order.Id, order.UserId), CancellationToken.None);

        var refund = await GetPersistedRefundAsync(order.Id);
        refund!.Id.Should().Be(firstRefundId);
    }

    public void Dispose()
        => _context.Dispose();

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

    private async Task<Domain.Order.Refund?> GetPersistedRefundAsync(Guid orderId)
    {
        _context.DbContext.ChangeTracker.Clear();
        var order = await _context.DbContext.Orders.FindAsync(orderId)
            ?? throw new InvalidOperationException("Order was not persisted.");
        await _context.DbContext.Entry(order).Reference(o => o.Refund).LoadAsync();
        return order.Refund;
    }
}