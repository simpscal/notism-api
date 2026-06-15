using FluentAssertions;

using Microsoft.Extensions.Logging;

using Notism.Application.Common.Services;
using Notism.Application.Order.MarkRefundFailed;
using Notism.Application.Tests.Common;
using Notism.Domain.Order.Enums;
using Notism.Infrastructure.Repositories;
using Notism.Shared.Exceptions;

using NSubstitute;

using DomainOrder = Notism.Domain.Order.Order;
using PaymentMethodEnum = Notism.Domain.Order.Enums.PaymentMethod;

namespace Notism.Application.Tests.Order.MarkRefundFailed;

public class MarkRefundFailedHandlerTests : IDisposable
{
    private readonly WriteHandlerContext _context;
    private readonly IMessages _messages;
    private readonly MarkRefundFailedHandler _handler;

    public MarkRefundFailedHandlerTests()
    {
        _context = new WriteHandlerContext();
        _messages = Substitute.For<IMessages>();
        _messages.RefundNotFound.Returns("Refund not found.");
        _messages.RefundNotProcessing.Returns("Refund not processing.");

        _handler = new MarkRefundFailedHandler(
            new OrderRepository(_context.DbContext),
            _context.DbContext,
            Substitute.For<ILogger<MarkRefundFailedHandler>>(),
            _messages);
    }

    [Fact]
    public async Task Handle_WhenRefundProcessing_TransitionsToFailedWithReason()
    {
        var order = await SeedOrderWithRefundAsync(RefundStatus.Processing);

        var result = await _handler.Handle(
            new MarkRefundFailedRequest { RefundId = order.Refund!.Id, Reason = "manual transfer rejected" },
            CancellationToken.None);

        result.Status.Should().Be("failed");
        result.FailureReason.Should().Be("manual transfer rejected");

        var refund = await GetPersistedRefundAsync(order.Id);
        refund.Status.Should().Be(RefundStatus.Failed);
        refund.FailureReason.Should().Be("manual transfer rejected");
    }

    [Fact]
    public async Task Handle_WhenRefundNotProcessing_ThrowsConflict()
    {
        var order = await SeedOrderWithRefundAsync(RefundStatus.Pending);

        var act = async () => await _handler.Handle(
            new MarkRefundFailedRequest { RefundId = order.Refund!.Id, Reason = "rejected" },
            CancellationToken.None);

        await act.Should().ThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task Handle_WhenRefundNotFound_ThrowsNotFound()
    {
        var act = async () => await _handler.Handle(
            new MarkRefundFailedRequest { RefundId = Guid.NewGuid(), Reason = "rejected" },
            CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    public void Dispose()
        => _context.Dispose();

    private async Task<DomainOrder> SeedOrderWithRefundAsync(RefundStatus status)
    {
        var order = DomainOrder.Create(Guid.NewGuid(), PaymentMethodEnum.Banking, new List<Guid>());
        order.AddItem(Domain.Order.OrderItem.Create(order.Id, Guid.NewGuid(), "Burger", unitPrice: 150_000m, discountPrice: null, quantity: 1));
        order.MarkAsPaid(DateTime.UtcNow);
        order.CreateRefund();

        if (status == RefundStatus.Processing)
        {
            order.MarkRefundProcessing();
        }

        order.ClearDomainEvents();
        await _context.SeedAsync(order);
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