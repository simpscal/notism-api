using FluentAssertions;

using MediatR;

using Microsoft.Extensions.Logging;

using Notism.Application.Common.Services;
using Notism.Application.Payment.HandleSepayWebhook;
using Notism.Application.Tests.Common;
using Notism.Domain.Order.Enums;
using Notism.Infrastructure.Repositories;

using NSubstitute;

using DomainOrder = Notism.Domain.Order.Order;
using PaymentMethodEnum = Notism.Domain.Order.Enums.PaymentMethod;

namespace Notism.Application.Tests.Payment.HandleSepayWebhook;

public class HandleSepayWebhookRefundTests : IDisposable
{
    private readonly WriteHandlerContext _context;
    private readonly HandleSepayWebhookHandler _handler;

    public HandleSepayWebhookRefundTests()
    {
        _context = new WriteHandlerContext();

        _handler = new HandleSepayWebhookHandler(
            new BankingCheckoutRepository(_context.DbContext),
            new OrderRepository(_context.DbContext),
            _context.DbContext,
            Substitute.For<ISender>(),
            Substitute.For<IPaymentNotifier>(),
            Substitute.For<ILogger<HandleSepayWebhookHandler>>());
    }

    [Fact]
    public async Task Handle_WhenOutboundMatchesProcessingRefundAndAmount_MarksRefundPaidAndCapturesReference()
    {
        var order = await SeedOrderWithProcessingRefundAsync(150_000m);
        var refundId = order.Refund!.Id;

        var request = new HandleSepayWebhookRequest
        {
            TransactionId = "SEPAY-TX-1",
            Amount = 150_000m,
            Content = refundId.ToString("N"),
            TransferType = "out",
            TransferredAt = DateTime.UtcNow,
        };

        await _handler.Handle(request, CancellationToken.None);

        var refund = await GetPersistedRefundAsync(order.Id);
        refund.Status.Should().Be(RefundStatus.Paid);
        refund.TransferReference.Should().Be("SEPAY-TX-1");
        refund.PaidAt.Should().NotBeNull();
    }

    [Fact]
    public async Task Handle_WhenOutboundRefundIdAtEndPrecededByBankTokens_MarksRefundPaid()
    {
        var order = await SeedOrderWithProcessingRefundAsync(150_000m);
        var refundId = order.Refund!.Id;

        var request = new HandleSepayWebhookRequest
        {
            TransactionId = "SEPAY-TX-END",
            Amount = 150_000m,
            Content = "EWX445876928 H21WVC4Q IBFT  " + refundId.ToString("N"),
            TransferType = "out",
            TransferredAt = DateTime.UtcNow,
        };

        await _handler.Handle(request, CancellationToken.None);

        var refund = await GetPersistedRefundAsync(order.Id);
        refund.Status.Should().Be(RefundStatus.Paid);
        refund.TransferReference.Should().Be("SEPAY-TX-END");
    }

    [Fact]
    public async Task Handle_WhenOutboundRefundIdInMiddleOfContent_MarksRefundPaid()
    {
        var order = await SeedOrderWithProcessingRefundAsync(150_000m);
        var refundId = order.Refund!.Id;

        var request = new HandleSepayWebhookRequest
        {
            TransactionId = "SEPAY-TX-MID",
            Amount = 150_000m,
            Content = "EWX445876928 " + refundId.ToString("N") + " IBFT",
            TransferType = "out",
            TransferredAt = DateTime.UtcNow,
        };

        await _handler.Handle(request, CancellationToken.None);

        var refund = await GetPersistedRefundAsync(order.Id);
        refund.Status.Should().Be(RefundStatus.Paid);
        refund.TransferReference.Should().Be("SEPAY-TX-MID");
    }

    [Fact]
    public async Task Handle_WhenOutboundContentHasNoValidGuid_NoOps()
    {
        var order = await SeedOrderWithProcessingRefundAsync(150_000m);

        var request = new HandleSepayWebhookRequest
        {
            TransactionId = "SEPAY-TX-2",
            Amount = 150_000m,
            Content = "TOOSHORT-invalid",
            TransferType = "out",
            TransferredAt = DateTime.UtcNow,
        };

        await _handler.Handle(request, CancellationToken.None);

        var refund = await GetPersistedRefundAsync(order.Id);
        refund.Status.Should().Be(RefundStatus.Processing);
    }

    [Fact]
    public async Task Handle_WhenOutboundRefundNotFound_NoOps()
    {
        var request = new HandleSepayWebhookRequest
        {
            TransactionId = "SEPAY-TX-3",
            Amount = 150_000m,
            Content = Guid.NewGuid().ToString("N"),
            TransferType = "out",
            TransferredAt = DateTime.UtcNow,
        };

        var act = async () => await _handler.Handle(request, CancellationToken.None);

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task Handle_WhenOutboundRefundAlreadyPaid_NoOps()
    {
        var order = DomainOrder.Create(Guid.NewGuid(), PaymentMethodEnum.Banking, new List<Guid>());
        order.AddItem(Domain.Order.OrderItem.Create(order.Id, Guid.NewGuid(), "Burger", unitPrice: 150_000m, discountPrice: null, quantity: 1));
        order.MarkAsPaid(DateTime.UtcNow);
        order.CreateRefund();
        order.MarkRefundProcessing();
        order.MarkRefundPaid("EARLIER-REF");
        await PersistAsync(order);
        var refundId = order.Refund!.Id;

        var request = new HandleSepayWebhookRequest
        {
            TransactionId = "SEPAY-TX-4",
            Amount = 150_000m,
            Content = refundId.ToString("N"),
            TransferType = "out",
            TransferredAt = DateTime.UtcNow,
        };

        await _handler.Handle(request, CancellationToken.None);

        var refund = await GetPersistedRefundAsync(order.Id);
        refund.Status.Should().Be(RefundStatus.Paid);
        refund.TransferReference.Should().Be("EARLIER-REF");
    }

    [Fact]
    public async Task Handle_WhenOutboundAmountMismatch_NoOps()
    {
        var order = await SeedOrderWithProcessingRefundAsync(150_000m);
        var refundId = order.Refund!.Id;

        var request = new HandleSepayWebhookRequest
        {
            TransactionId = "SEPAY-TX-5",
            Amount = 100_000m,
            Content = refundId.ToString("N"),
            TransferType = "out",
            TransferredAt = DateTime.UtcNow,
        };

        await _handler.Handle(request, CancellationToken.None);

        var refund = await GetPersistedRefundAsync(order.Id);
        refund.Status.Should().Be(RefundStatus.Processing);
        refund.TransferReference.Should().BeNull();
    }

    public void Dispose()
        => _context.Dispose();

    private async Task<DomainOrder> SeedOrderWithProcessingRefundAsync(decimal amount)
    {
        var order = DomainOrder.Create(Guid.NewGuid(), PaymentMethodEnum.Banking, new List<Guid>());
        order.AddItem(Domain.Order.OrderItem.Create(order.Id, Guid.NewGuid(), "Burger", unitPrice: amount, discountPrice: null, quantity: 1));
        order.MarkAsPaid(DateTime.UtcNow);
        order.CreateRefund();
        order.MarkRefundProcessing();
        await PersistAsync(order);
        return order;
    }

    private async Task PersistAsync(DomainOrder order)
    {
        order.ClearDomainEvents();
        await _context.SeedAsync(order);
    }

    private async Task<Domain.Order.Refund> GetPersistedRefundAsync(Guid orderId)
    {
        _context.DbContext.ChangeTracker.Clear();
        var order = await _context.DbContext.Orders
            .FindAsync(orderId)
            ?? throw new InvalidOperationException("Order was not persisted.");
        await _context.DbContext.Entry(order).Reference(o => o.Refund).LoadAsync();
        return order.Refund ?? throw new InvalidOperationException("Refund was not persisted.");
    }
}