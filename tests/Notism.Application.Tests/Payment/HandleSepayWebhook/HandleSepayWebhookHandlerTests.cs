using FluentAssertions;

using MediatR;

using Microsoft.Extensions.Logging;

using Notism.Application.Common.Interfaces;
using Notism.Application.Order.CreateOrder;
using Notism.Application.Payment.HandleSepayWebhook;
using Notism.Domain.Common.Specifications;
using Notism.Domain.Order;
using Notism.Domain.Order.Enums;
using Notism.Domain.Payment;

using NSubstitute;

namespace Notism.Application.Tests.Payment.HandleSepayWebhook;

public class HandleSepayWebhookHandlerTests
{
    private readonly IBankingCheckoutRepository _bankingCheckoutRepository;
    private readonly IOrderRepository _orderRepository;
    private readonly ISender _sender;
    private readonly INotificationService _notificationService;
    private readonly ILogger<HandleSepayWebhookHandler> _logger;
    private readonly HandleSepayWebhookHandler _handler;

    public HandleSepayWebhookHandlerTests()
    {
        _bankingCheckoutRepository = Substitute.For<IBankingCheckoutRepository>();
        _orderRepository = Substitute.For<IOrderRepository>();
        _sender = Substitute.For<ISender>();
        _notificationService = Substitute.For<INotificationService>();
        _logger = Substitute.For<ILogger<HandleSepayWebhookHandler>>();

        _handler = new HandleSepayWebhookHandler(
            _bankingCheckoutRepository,
            _orderRepository,
            _sender,
            _notificationService,
            _logger);
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

        var orderId = Guid.NewGuid();
        var order = Domain.Order.Order.Create(userId, PaymentMethod.Banking, cartItemIds);

        _bankingCheckoutRepository
            .FindByExpressionAsync(Arg.Any<FilterSpecification<BankingCheckout>>())
            .Returns(checkout);

        _sender
            .Send(Arg.Any<CreateOrderRequest>(), Arg.Any<CancellationToken>())
            .Returns(new CreateOrderResponse { OrderId = orderId });

        _orderRepository
            .FindByExpressionAsync(Arg.Any<FilterSpecification<Domain.Order.Order>>())
            .Returns(order);

        var hex32 = checkoutId.ToString("N");
        var request = new HandleSepayWebhookRequest
        {
            TransactionId = "99001",
            Amount = totalAmount,
            Content = hex32 + " some extra content",
            TransferredAt = transferredAt,
        };

        await _handler.Handle(request, CancellationToken.None);

        order.PaymentStatus.Should().Be(Domain.Payment.Enums.PaymentStatus.Paid);
        order.PaidAt.Should().Be(transferredAt);
        checkout.IsUsed.Should().BeTrue();
        await _orderRepository.Received(1).SaveChangesAsync();
        await _bankingCheckoutRepository.Received(1).SaveChangesAsync();
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

        await _bankingCheckoutRepository.DidNotReceive().FindByExpressionAsync(Arg.Any<FilterSpecification<BankingCheckout>>());
        await _bankingCheckoutRepository.DidNotReceive().SaveChangesAsync();
    }

    [Fact]
    public async Task Handle_WhenCheckoutNotFound_ReturnsWithoutCreatingOrder()
    {
        var checkoutId = Guid.NewGuid();

        _bankingCheckoutRepository
            .FindByExpressionAsync(Arg.Any<FilterSpecification<BankingCheckout>>())
            .Returns((BankingCheckout?)null);

        var request = new HandleSepayWebhookRequest
        {
            TransactionId = "99003",
            Amount = 50_000m,
            Content = checkoutId.ToString("N") + " extra",
            TransferredAt = DateTime.UtcNow,
        };

        await _handler.Handle(request, CancellationToken.None);

        await _sender.DidNotReceive().Send(Arg.Any<CreateOrderRequest>(), Arg.Any<CancellationToken>());
        await _bankingCheckoutRepository.DidNotReceive().SaveChangesAsync();
    }

    [Fact]
    public async Task Handle_WhenCheckoutAlreadyUsed_ReturnsWithoutCreatingOrder()
    {
        var userId = Guid.NewGuid();
        var checkoutId = Guid.NewGuid();
        var checkout = BankingCheckout.Create(userId, new List<Guid>(), 100_000m);
        checkout.Id = checkoutId;
        checkout.MarkAsUsed();

        _bankingCheckoutRepository
            .FindByExpressionAsync(Arg.Any<FilterSpecification<BankingCheckout>>())
            .Returns(checkout);

        var request = new HandleSepayWebhookRequest
        {
            TransactionId = "99004",
            Amount = 100_000m,
            Content = checkoutId.ToString("N") + " duplicate",
            TransferredAt = DateTime.UtcNow,
        };

        await _handler.Handle(request, CancellationToken.None);

        await _sender.DidNotReceive().Send(Arg.Any<CreateOrderRequest>(), Arg.Any<CancellationToken>());
        await _bankingCheckoutRepository.DidNotReceive().SaveChangesAsync();
    }

    [Fact]
    public async Task Handle_WhenAmountDoesNotMatchCheckoutTotal_SendsFailureNotificationAndDoesNotCreateOrder()
    {
        var userId = Guid.NewGuid();
        var checkoutId = Guid.NewGuid();
        var checkout = BankingCheckout.Create(userId, new List<Guid> { Guid.NewGuid() }, 150_000m);
        checkout.Id = checkoutId;

        _bankingCheckoutRepository
            .FindByExpressionAsync(Arg.Any<FilterSpecification<BankingCheckout>>())
            .Returns(checkout);

        var request = new HandleSepayWebhookRequest
        {
            TransactionId = "99005",
            Amount = 100_000m,
            Content = checkoutId.ToString("N") + " mismatch",
            TransferredAt = DateTime.UtcNow,
        };

        await _handler.Handle(request, CancellationToken.None);

        await _notificationService.Received(1).NotifyPaymentFailureAsync(
            Guid.Empty,
            userId,
            Arg.Any<CancellationToken>());
        await _sender.DidNotReceive().Send(Arg.Any<CreateOrderRequest>(), Arg.Any<CancellationToken>());
        await _bankingCheckoutRepository.DidNotReceive().SaveChangesAsync();
    }
}
