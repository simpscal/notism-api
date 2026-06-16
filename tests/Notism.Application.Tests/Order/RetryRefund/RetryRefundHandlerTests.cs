using FluentAssertions;

using Microsoft.Extensions.Logging;

using Notism.Application.Common.Services;
using Notism.Application.Order.RetryRefund;
using Notism.Application.Tests.Common;
using Notism.Domain.Order.Enums;
using Notism.Infrastructure.Repositories;
using Notism.Shared.Exceptions;

using NSubstitute;

using DomainOrder = Notism.Domain.Order.Order;
using PaymentMethodEnum = Notism.Domain.Order.Enums.PaymentMethod;

namespace Notism.Application.Tests.Order.RetryRefund;

public class RetryRefundHandlerTests : IDisposable
{
    private readonly WriteHandlerContext _context;
    private readonly IMessages _messages;
    private readonly RetryRefundHandler _handler;

    public RetryRefundHandlerTests()
    {
        _context = new WriteHandlerContext();
        _messages = Substitute.For<IMessages>();
        _messages.RefundNotFound.Returns("Refund not found.");
        _messages.RefundNotFailed.Returns("Refund not failed.");

        _handler = new RetryRefundHandler(
            new OrderRepository(_context.DbContext),
            _context.DbContext,
            Substitute.For<ILogger<RetryRefundHandler>>(),
            _messages);
    }

    [Fact]
    public async Task Handle_WhenRefundFailed_TransitionsToProcessing()
    {
        var order = await SeedFailedRefundAsync();

        var result = await _handler.Handle(
            new RetryRefundRequest { RefundId = order.Refund!.Id },
            CancellationToken.None);

        result.Status.Should().Be("processing");

        var refund = await GetPersistedRefundAsync(order.Id);
        refund.Status.Should().Be(RefundStatus.Processing);
        refund.FailureReason.Should().BeNull();
    }

    [Fact]
    public async Task Handle_WhenRefundNotFailed_ThrowsConflict()
    {
        var order = await SeedPendingRefundAsync();

        var act = async () => await _handler.Handle(
            new RetryRefundRequest { RefundId = order.Refund!.Id },
            CancellationToken.None);

        await act.Should().ThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task Handle_WhenRefundPaid_ThrowsConflict()
    {
        var order = await SeedPaidRefundAsync();

        var act = async () => await _handler.Handle(
            new RetryRefundRequest { RefundId = order.Refund!.Id },
            CancellationToken.None);

        await act.Should().ThrowAsync<ConflictException>();

        var refund = await GetPersistedRefundAsync(order.Id);
        refund.Status.Should().Be(RefundStatus.Paid);
    }

    [Fact]
    public async Task Handle_WhenRefundNotFound_ThrowsNotFound()
    {
        var act = async () => await _handler.Handle(
            new RetryRefundRequest { RefundId = Guid.NewGuid() },
            CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    public void Dispose()
        => _context.Dispose();

    private async Task<DomainOrder> SeedFailedRefundAsync()
    {
        var order = NewPaidOrderWithRefund();
        order.MarkRefundProcessing();
        order.MarkRefundFailed("provider declined");
        order.ClearDomainEvents();
        await _context.SeedAsync(order);
        return order;
    }

    private async Task<DomainOrder> SeedPendingRefundAsync()
    {
        var order = NewPaidOrderWithRefund();
        order.ClearDomainEvents();
        await _context.SeedAsync(order);
        return order;
    }

    private async Task<DomainOrder> SeedPaidRefundAsync()
    {
        var order = NewPaidOrderWithRefund();
        order.MarkRefundProcessing();
        order.MarkRefundPaid("TXN-RETRY");
        order.ClearDomainEvents();
        await _context.SeedAsync(order);
        return order;
    }

    private static DomainOrder NewPaidOrderWithRefund()
    {
        var order = DomainOrder.Create(Guid.NewGuid(), PaymentMethodEnum.Banking, new List<Guid>());
        order.AddItem(Domain.Order.OrderItem.Create(order.Id, Guid.NewGuid(), "Burger", unitPrice: 150_000m, discountPrice: null, quantity: 1));
        order.MarkAsPaid(DateTime.UtcNow);
        order.CreateRefund();
        return order;
    }

    private async Task<Domain.Order.Refund> GetPersistedRefundAsync(Guid orderId)
    {
        _context.DbContext.ChangeTracker.Clear();
        var order = await _context.DbContext.Orders.FindAsync(orderId)
            ?? throw new InvalidOperationException("Order was not persisted.");
        await _context.DbContext.Entry(order).Reference(o => o.Refund).LoadAsync();
        return order.Refund ?? throw new InvalidOperationException("Refund was not persisted.");
    }
}