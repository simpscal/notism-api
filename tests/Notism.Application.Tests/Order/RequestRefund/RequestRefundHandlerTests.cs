using FluentAssertions;

using Microsoft.Extensions.Logging;

using Notism.Application.Common.Services;
using Notism.Application.Order.RequestRefund;
using Notism.Application.Tests.Common;
using Notism.Domain.Order.Enums;
using Notism.Infrastructure.Repositories;
using Notism.Shared.Exceptions;

using NSubstitute;

using DomainOrder = Notism.Domain.Order.Order;
using PaymentMethodEnum = Notism.Domain.Order.Enums.PaymentMethod;

namespace Notism.Application.Tests.Order.RequestRefund;

public class RequestRefundHandlerTests : IDisposable
{
    private readonly WriteHandlerContext _context;
    private readonly RequestRefundHandler _handler;

    public RequestRefundHandlerTests()
    {
        _context = new WriteHandlerContext();

        var messages = Substitute.For<IMessages>();
        messages.OrderNotFound.Returns("Order not found.");
        messages.RefundNotEligible.Returns("Refund not eligible.");
        messages.RefundAlreadyRequested.Returns("Refund already requested.");

        _handler = new RequestRefundHandler(
            new OrderRepository(_context.DbContext),
            _context.DbContext,
            Substitute.For<ILogger<RequestRefundHandler>>(),
            messages);
    }

    [Fact]
    public async Task Handle_WhenDeliveredBankingOrderWithin24h_CreatesPendingFullTotalRefund()
    {
        var order = await SeedOrderAsync(
            PaymentMethodEnum.Banking,
            paid: true,
            deliveredAt: DateTime.UtcNow.AddHours(-2));

        var result = await _handler.Handle(Request(order), CancellationToken.None);

        result.Status.Should().Be("pending");
        result.Amount.Should().Be(order.TotalAmount);

        var persisted = await ReloadAsync(order.Id);
        persisted.Refund.Should().NotBeNull();
        persisted.Refund!.Status.Should().Be(RefundStatus.Pending);
        persisted.Refund.Amount.Should().Be(order.TotalAmount);
    }

    [Fact]
    public async Task Handle_WhenOrderNotDelivered_ThrowsBadRequest()
    {
        var order = await SeedOrderAsync(
            PaymentMethodEnum.Banking,
            paid: true,
            deliveredAt: null);

        var act = () => _handler.Handle(Request(order), CancellationToken.None);

        await act.Should().ThrowAsync<ResultFailureException>();
        var persisted = await ReloadAsync(order.Id);
        persisted.Refund.Should().BeNull();
    }

    [Fact]
    public async Task Handle_WhenNotBankingOrder_ThrowsBadRequest()
    {
        var order = await SeedOrderAsync(
            PaymentMethodEnum.CashOnDelivery,
            paid: true,
            deliveredAt: DateTime.UtcNow.AddHours(-2));

        var act = () => _handler.Handle(Request(order), CancellationToken.None);

        await act.Should().ThrowAsync<ResultFailureException>();
        var persisted = await ReloadAsync(order.Id);
        persisted.Refund.Should().BeNull();
    }

    [Fact]
    public async Task Handle_WhenDeliveredMoreThan24hAgo_ThrowsBadRequest()
    {
        var order = await SeedOrderAsync(
            PaymentMethodEnum.Banking,
            paid: true,
            deliveredAt: DateTime.UtcNow.AddHours(-25));

        var act = () => _handler.Handle(Request(order), CancellationToken.None);

        await act.Should().ThrowAsync<ResultFailureException>();
        var persisted = await ReloadAsync(order.Id);
        persisted.Refund.Should().BeNull();
    }

    [Fact]
    public async Task Handle_WhenRefundAlreadyExists_ThrowsConflict()
    {
        var order = await SeedOrderAsync(
            PaymentMethodEnum.Banking,
            paid: true,
            deliveredAt: DateTime.UtcNow.AddHours(-2),
            withRefund: true);

        var act = () => _handler.Handle(Request(order), CancellationToken.None);

        await act.Should().ThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task Handle_WhenOrderNotFound_ThrowsResultFailureException()
    {
        var act = () => _handler.Handle(
            new RequestRefundRequest { OrderId = Guid.NewGuid(), UserId = Guid.NewGuid() },
            CancellationToken.None);

        await act.Should().ThrowAsync<ResultFailureException>();
    }

    [Fact]
    public async Task Handle_WhenOrderBelongsToAnotherUser_ThrowsResultFailureException()
    {
        var order = await SeedOrderAsync(
            PaymentMethodEnum.Banking,
            paid: true,
            deliveredAt: DateTime.UtcNow.AddHours(-2));

        var act = () => _handler.Handle(
            new RequestRefundRequest { OrderId = order.Id, UserId = Guid.NewGuid() },
            CancellationToken.None);

        await act.Should().ThrowAsync<ResultFailureException>();
    }

    public void Dispose()
        => _context.Dispose();

    private static RequestRefundRequest Request(DomainOrder order)
        => new() { OrderId = order.Id, UserId = order.UserId };

    private async Task<DomainOrder> SeedOrderAsync(
        PaymentMethodEnum paymentMethod,
        bool paid,
        DateTime? deliveredAt,
        bool withRefund = false)
    {
        var order = DomainOrder.Create(Guid.NewGuid(), paymentMethod, new List<Guid>());
        order.AddItem(Domain.Order.OrderItem.Create(order.Id, Guid.NewGuid(), "Burger", unitPrice: 150_000m, discountPrice: null, quantity: 1));

        if (paid)
        {
            order.MarkAsPaid(DateTime.UtcNow);
        }

        if (deliveredAt.HasValue)
        {
            order.RecordDeliveredAt(deliveredAt.Value);
        }

        if (withRefund)
        {
            order.RequestRefund();
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