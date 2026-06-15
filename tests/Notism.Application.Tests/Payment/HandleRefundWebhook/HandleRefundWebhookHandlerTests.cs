using FluentAssertions;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Notism.Application.Payment.HandleRefundWebhook;
using Notism.Application.Tests.Common;
using Notism.Domain.Order.Enums;
using Notism.Infrastructure.Repositories;
using Notism.Shared.Configuration;
using Notism.Shared.Exceptions;

using NSubstitute;

using DomainOrder = Notism.Domain.Order.Order;
using DomainOrderItem = Notism.Domain.Order.OrderItem;

namespace Notism.Application.Tests.Payment.HandleRefundWebhook;

public class HandleRefundWebhookHandlerTests : IDisposable
{
    private const string WebhookSecret = "test-webhook-secret";

    private readonly WriteHandlerContext _context;
    private readonly HandleRefundWebhookHandler _handler;

    public HandleRefundWebhookHandlerTests()
    {
        _context = new WriteHandlerContext();

        var settings = Options.Create(new BankingRefundSettings { WebhookSecret = WebhookSecret });

        _handler = new HandleRefundWebhookHandler(
            new OrderRepository(_context.DbContext),
            _context.DbContext,
            settings,
            Substitute.For<ILogger<HandleRefundWebhookHandler>>());
    }

    [Fact]
    public async Task Handle_WhenSecretIsInvalid_ThrowsResultFailure()
    {
        var request = new HandleRefundWebhookRequest
        {
            Secret = "wrong-secret",
            TransferReference = "REF-1",
            Status = "success",
        };

        var act = () => _handler.Handle(request, CancellationToken.None);

        await act.Should().ThrowAsync<ResultFailureException>();
    }

    [Fact]
    public async Task Handle_WhenNoRefundMatchesTransferReference_ThrowsNotFound()
    {
        var request = new HandleRefundWebhookRequest
        {
            Secret = WebhookSecret,
            TransferReference = "REF-UNKNOWN",
            Status = "success",
        };

        var act = () => _handler.Handle(request, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenRefundIsProcessingAndStatusSuccess_MarksRefundPaid()
    {
        var order = CreateProcessingRefundOrder("REF-PAID");
        await _context.SeedAsync(order);

        var request = new HandleRefundWebhookRequest
        {
            Secret = WebhookSecret,
            TransferReference = "REF-PAID",
            Status = "success",
        };

        await _handler.Handle(request, CancellationToken.None);

        _context.DbContext.ChangeTracker.Clear();
        var refund = _context.DbContext.Refunds.Single(r => r.OrderId == order.Id);
        refund.Status.Should().Be(RefundStatus.Paid);
        refund.PaidAt.Should().NotBeNull();
    }

    [Fact]
    public async Task Handle_WhenRefundIsProcessingAndStatusFailure_MarksRefundFailed()
    {
        var order = CreateProcessingRefundOrder("REF-FAIL");
        await _context.SeedAsync(order);

        var request = new HandleRefundWebhookRequest
        {
            Secret = WebhookSecret,
            TransferReference = "REF-FAIL",
            Status = "failure",
            FailureReason = "Account closed",
        };

        await _handler.Handle(request, CancellationToken.None);

        _context.DbContext.ChangeTracker.Clear();
        var refund = _context.DbContext.Refunds.Single(r => r.OrderId == order.Id);
        refund.Status.Should().Be(RefundStatus.Failed);
        refund.FailureReason.Should().Be("Account closed");
    }

    [Fact]
    public async Task Handle_WhenRefundAlreadyPaid_NoOpsOnRedelivery()
    {
        var order = CreateProcessingRefundOrder("REF-TERMINAL");
        order.MarkRefundPaid();
        await _context.SeedAsync(order);

        var request = new HandleRefundWebhookRequest
        {
            Secret = WebhookSecret,
            TransferReference = "REF-TERMINAL",
            Status = "failure",
            FailureReason = "Should be ignored",
        };

        await _handler.Handle(request, CancellationToken.None);

        _context.DbContext.ChangeTracker.Clear();
        var refund = _context.DbContext.Refunds.Single(r => r.OrderId == order.Id);
        refund.Status.Should().Be(RefundStatus.Paid);
        refund.FailureReason.Should().BeNull();
    }

    public void Dispose()
        => _context.Dispose();

    private static DomainOrder CreateProcessingRefundOrder(string transferReference)
    {
        var order = DomainOrder.Create(Guid.NewGuid(), PaymentMethod.Banking, new List<Guid>());
        order.AddItem(DomainOrderItem.Create(order.Id, Guid.NewGuid(), "Burger", unitPrice: 50m, discountPrice: null, quantity: 2));
        order.MarkAsPaid(DateTime.UtcNow);
        order.CreateRefund();
        order.MarkRefundProcessing(transferReference);
        return order;
    }
}