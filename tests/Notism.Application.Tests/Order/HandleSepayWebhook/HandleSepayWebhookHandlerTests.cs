using FluentAssertions;

using MediatR;

using Microsoft.Extensions.Logging;

using Notism.Application.Common.Services;
using Notism.Application.Order.CreateOrder;
using Notism.Application.Order.HandleSepayWebhook;
using Notism.Application.Tests.Common;
using Notism.Domain.Order;
using Notism.Domain.Order.Enums;
using Notism.Infrastructure.Repositories;

using NSubstitute;

namespace Notism.Application.Tests.Order.HandleSepayWebhook;

public class HandleSepayWebhookHandlerTests : IDisposable
{
    private readonly WriteHandlerContext _context;
    private readonly ISender _sender;
    private readonly IPaymentNotifier _paymentNotifier;
    private readonly HandleSepayWebhookHandler _handler;

    public HandleSepayWebhookHandlerTests()
    {
        _context = new WriteHandlerContext();
        _sender = Substitute.For<ISender>();
        _paymentNotifier = Substitute.For<IPaymentNotifier>();

        _handler = new HandleSepayWebhookHandler(
            new BankingCheckoutRepository(_context.DbContext),
            new OrderRepository(_context.DbContext),
            _context.DbContext,
            _sender,
            _paymentNotifier,
            Substitute.For<ILogger<HandleSepayWebhookHandler>>());
    }

    [Fact]
    public async Task Handle_WhenCheckoutIdParsesAndAmountMatchesAndOrderIsCreated_MarksOrderAsPaidAndMarksCheckoutAsUsed()
    {
        var userId = Guid.NewGuid();
        var checkoutId = Guid.NewGuid();
        var cartItemIds = new List<Guid> { Guid.NewGuid() };
        var totalAmount = 150_000m;
        var transferredAt = new DateTime(2026, 4, 5, 10, 0, 0, DateTimeKind.Utc);

        var checkout = BankingCheckout.Create(userId, cartItemIds, totalAmount);
        checkout.Id = checkoutId;

        var order = Domain.Order.Order.Create(userId, PaymentMethod.Banking, cartItemIds);
        await _context.SeedAsync(checkout, order);

        _sender
            .Send(Arg.Any<CreateOrderRequest>(), Arg.Any<CancellationToken>())
            .Returns(new CreateOrderResponse { OrderId = order.Id });

        var request = new HandleSepayWebhookRequest
        {
            TransactionId = "99001",
            Amount = totalAmount,
            Content = checkoutId.ToString("N"),
            TransferredAt = transferredAt,
        };

        await _handler.Handle(request, CancellationToken.None);

        _context.DbContext.ChangeTracker.Clear();
        var persistedOrder = _context.DbContext.Orders.Single(o => o.Id == order.Id);
        persistedOrder.PaymentStatus.Should().Be(PaymentStatus.Paid);
        persistedOrder.PaidAt.Should().Be(transferredAt);
        _context.DbContext.BankingCheckouts.Single(c => c.Id == checkoutId).IsUsed.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WhenCheckoutIdAtEndPrecededByBankTokens_MarksOrderAsPaidAndMarksCheckoutAsUsed()
    {
        var userId = Guid.NewGuid();
        var checkoutId = Guid.NewGuid();
        var cartItemIds = new List<Guid> { Guid.NewGuid() };
        var totalAmount = 150_000m;
        var transferredAt = new DateTime(2026, 4, 5, 10, 0, 0, DateTimeKind.Utc);

        var checkout = BankingCheckout.Create(userId, cartItemIds, totalAmount);
        checkout.Id = checkoutId;

        var order = Domain.Order.Order.Create(userId, PaymentMethod.Banking, cartItemIds);
        await _context.SeedAsync(checkout, order);

        _sender
            .Send(Arg.Any<CreateOrderRequest>(), Arg.Any<CancellationToken>())
            .Returns(new CreateOrderResponse { OrderId = order.Id });

        var request = new HandleSepayWebhookRequest
        {
            TransactionId = "99010",
            Amount = totalAmount,
            Content = "EWX445876928 H21WVC4Q IBFT  " + checkoutId.ToString("N"),
            TransferredAt = transferredAt,
        };

        await _handler.Handle(request, CancellationToken.None);

        _context.DbContext.ChangeTracker.Clear();
        var persistedOrder = _context.DbContext.Orders.Single(o => o.Id == order.Id);
        persistedOrder.PaymentStatus.Should().Be(PaymentStatus.Paid);
        persistedOrder.PaidAt.Should().Be(transferredAt);
        _context.DbContext.BankingCheckouts.Single(c => c.Id == checkoutId).IsUsed.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WhenContentDoesNotContainValidGuid_ReturnsWithoutLookingUpCheckout()
    {
        var request = new HandleSepayWebhookRequest
        {
            TransactionId = "99002",
            Amount = 100_000m,
            Content = "TOOSHORT-invalid-content",
            TransferredAt = DateTime.UtcNow,
        };

        await _handler.Handle(request, CancellationToken.None);

        await _sender.DidNotReceive().Send(Arg.Any<CreateOrderRequest>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenCheckoutNotFound_ReturnsWithoutCreatingOrder()
    {
        var request = new HandleSepayWebhookRequest
        {
            TransactionId = "99003",
            Amount = 50_000m,
            Content = Guid.NewGuid().ToString("N"),
            TransferredAt = DateTime.UtcNow,
        };

        await _handler.Handle(request, CancellationToken.None);

        await _sender.DidNotReceive().Send(Arg.Any<CreateOrderRequest>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenCheckoutAlreadyUsed_ReturnsWithoutCreatingOrder()
    {
        var userId = Guid.NewGuid();
        var checkoutId = Guid.NewGuid();
        var checkout = BankingCheckout.Create(userId, new List<Guid>(), 100_000m);
        checkout.Id = checkoutId;
        checkout.MarkAsUsed();
        await _context.SeedAsync(checkout);

        var request = new HandleSepayWebhookRequest
        {
            TransactionId = "99004",
            Amount = 100_000m,
            Content = checkoutId.ToString("N"),
            TransferredAt = DateTime.UtcNow,
        };

        await _handler.Handle(request, CancellationToken.None);

        await _sender.DidNotReceive().Send(Arg.Any<CreateOrderRequest>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenAmountDoesNotMatchCheckoutTotal_SendsFailureNotificationAndDoesNotCreateOrder()
    {
        var userId = Guid.NewGuid();
        var checkoutId = Guid.NewGuid();
        var checkout = BankingCheckout.Create(userId, new List<Guid> { Guid.NewGuid() }, 150_000m);
        checkout.Id = checkoutId;
        await _context.SeedAsync(checkout);

        var request = new HandleSepayWebhookRequest
        {
            TransactionId = "99005",
            Amount = 100_000m,
            Content = checkoutId.ToString("N"),
            TransferredAt = DateTime.UtcNow,
        };

        await _handler.Handle(request, CancellationToken.None);

        await _paymentNotifier.Received(1).NotifyPaymentFailureAsync(
            Guid.Empty,
            userId,
            Arg.Any<CancellationToken>());
        await _sender.DidNotReceive().Send(Arg.Any<CreateOrderRequest>(), Arg.Any<CancellationToken>());
    }

    public void Dispose()
        => _context.Dispose();
}