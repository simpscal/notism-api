using FluentAssertions;

using Microsoft.Extensions.Logging;

using Notism.Application.Payment.HandleSepayWebhook;
using Notism.Domain.Common.Specifications;
using Notism.Domain.Order;
using Notism.Domain.Order.Enums;
using Notism.Domain.Payment.Enums;

using NSubstitute;

namespace Notism.Application.Tests.Payment.HandleSepayWebhook;

public class HandleSepayWebhookHandlerTests
{
    private readonly IOrderRepository _orderRepository;
    private readonly ILogger<HandleSepayWebhookHandler> _logger;
    private readonly HandleSepayWebhookHandler _handler;

    public HandleSepayWebhookHandlerTests()
    {
        _orderRepository = Substitute.For<IOrderRepository>();
        _logger = Substitute.For<ILogger<HandleSepayWebhookHandler>>();
        _handler = new HandleSepayWebhookHandler(_orderRepository, _logger);
    }

    [Fact]
    public async Task Handle_WhenContentContainsSlugIdAsFirstSegmentAndAmountMatches_MarksOrderAsPaid()
    {
        var userId = Guid.NewGuid();
        var order = Domain.Order.Order.Create(userId, PaymentMethod.Banking, new List<Guid>());
        var slugBody = order.SlugId[4..]; // strips "ORD-"
        var transferredAt = new DateTime(2026, 4, 5, 10, 0, 0, DateTimeKind.Utc);

        _orderRepository
            .FindByExpressionAsync(Arg.Any<FilterSpecification<Domain.Order.Order>>())
            .Returns(order);

        // SePay content format: "<SlugBody>-<date>-<time> <rest>"
        var request = new HandleSepayWebhookRequest
        {
            TransactionId = "52408910",
            Amount = order.TotalAmount,
            Content = $"{slugBody}-210426-15:08:27 6111ASCB02QIZ3BI CKN 179827",
            TransferredAt = transferredAt,
        };

        await _handler.Handle(request, CancellationToken.None);

        order.PaymentStatus.Should().Be(PaymentStatus.Paid);
        order.PaidAt.Should().Be(transferredAt);
        await _orderRepository.Received(1).SaveChangesAsync();
    }

    [Fact]
    public async Task Handle_WhenContentIsEmptyOrHasNoSegments_ReturnsWithoutSavingAndDoesNotCallRepository()
    {
        var request = new HandleSepayWebhookRequest
        {
            TransactionId = "52408911",
            Amount = 100_000,
            Content = string.Empty,
            TransferredAt = DateTime.UtcNow,
        };

        await _handler.Handle(request, CancellationToken.None);

        await _orderRepository.DidNotReceive().FindByExpressionAsync(Arg.Any<FilterSpecification<Domain.Order.Order>>());
        await _orderRepository.DidNotReceive().SaveChangesAsync();
    }

    [Fact]
    public async Task Handle_WhenOrderNotFound_ReturnsWithoutSaving()
    {
        _orderRepository
            .FindByExpressionAsync(Arg.Any<FilterSpecification<Domain.Order.Order>>())
            .Returns((Domain.Order.Order?)null);

        var request = new HandleSepayWebhookRequest
        {
            TransactionId = "52408912",
            Amount = 50_000,
            Content = "MISSING123-210426-15:08:27 extra data",
            TransferredAt = DateTime.UtcNow,
        };

        await _handler.Handle(request, CancellationToken.None);

        await _orderRepository.DidNotReceive().SaveChangesAsync();
    }

    [Fact]
    public async Task Handle_WhenOrderAlreadyPaid_ReturnsWithoutSavingAgain()
    {
        var userId = Guid.NewGuid();
        var order = Domain.Order.Order.Create(userId, PaymentMethod.Banking, new List<Guid>());
        order.MarkAsPaid(DateTime.UtcNow.AddMinutes(-5));
        var slugBody = order.SlugId[4..]; // strips "ORD-"

        _orderRepository
            .FindByExpressionAsync(Arg.Any<FilterSpecification<Domain.Order.Order>>())
            .Returns(order);

        var request = new HandleSepayWebhookRequest
        {
            TransactionId = "52408913",
            Amount = order.TotalAmount,
            Content = $"{slugBody}-210426-15:08:27 duplicate webhook",
            TransferredAt = DateTime.UtcNow,
        };

        await _handler.Handle(request, CancellationToken.None);

        await _orderRepository.DidNotReceive().SaveChangesAsync();
        order.PaidAt.Should().NotBe(request.TransferredAt);
    }

    [Fact]
    public async Task Handle_WhenAmountDoesNotMatchOrderTotal_MarksOrderAsFailedAndSaves()
    {
        var userId = Guid.NewGuid();
        var order = Domain.Order.Order.Create(userId, PaymentMethod.Banking, new List<Guid>());
        var slugBody = order.SlugId[4..]; // strips "ORD-"

        _orderRepository
            .FindByExpressionAsync(Arg.Any<FilterSpecification<Domain.Order.Order>>())
            .Returns(order);

        var request = new HandleSepayWebhookRequest
        {
            TransactionId = "52408914",
            Amount = order.TotalAmount + 1,
            Content = $"{slugBody}-210426-15:08:27 amount mismatch",
            TransferredAt = DateTime.UtcNow,
        };

        await _handler.Handle(request, CancellationToken.None);

        order.PaymentStatus.Should().Be(PaymentStatus.Failed);
        await _orderRepository.Received(1).SaveChangesAsync();
    }

    [Fact]
    public async Task Handle_WhenPreviousPaymentFailedAndNewTransferSucceeds_MarksOrderAsPaid()
    {
        var userId = Guid.NewGuid();
        var order = Domain.Order.Order.Create(userId, PaymentMethod.Banking, new List<Guid>());
        var slugBody = order.SlugId[4..]; // strips "ORD-"
        var transferredAt = new DateTime(2026, 4, 21, 15, 8, 28, DateTimeKind.Utc);

        _orderRepository
            .FindByExpressionAsync(Arg.Any<FilterSpecification<Domain.Order.Order>>())
            .Returns(order);

        // Simulate a prior failed attempt
        order.MarkAsFailed();
        order.PaymentStatus.Should().Be(PaymentStatus.Failed);

        var request = new HandleSepayWebhookRequest
        {
            TransactionId = "52408910",
            Amount = order.TotalAmount,
            Content = $"{slugBody}-210421-15:08:28 6111ASCB02QIZ3BI",
            TransferredAt = transferredAt,
        };

        await _handler.Handle(request, CancellationToken.None);

        order.PaymentStatus.Should().Be(PaymentStatus.Paid);
        order.PaidAt.Should().Be(transferredAt);
        await _orderRepository.Received(1).SaveChangesAsync();
    }
}
