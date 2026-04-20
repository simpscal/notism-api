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
    public async Task Handle_WhenDescriptionContainsSlugIdAndAmountMatches_MarksOrderAsPaid()
    {
        var userId = Guid.NewGuid();
        var order = Domain.Order.Order.Create(userId, PaymentMethod.Banking, new List<Guid>());
        var transferredAt = new DateTime(2026, 4, 5, 10, 0, 0, DateTimeKind.Utc);

        _orderRepository
            .FindByExpressionAsync(Arg.Any<FilterSpecification<Domain.Order.Order>>())
            .Returns(order);

        var request = new HandleSepayWebhookRequest
        {
            TransactionId = "TXN-001",
            Amount = order.TotalAmount,
            Description = $"Chuyen khoan {order.SlugId} thanh toan",
            TransferredAt = transferredAt,
        };

        await _handler.Handle(request, CancellationToken.None);

        order.PaymentStatus.Should().Be(PaymentStatus.Paid);
        order.PaidAt.Should().Be(transferredAt);
        await _orderRepository.Received(1).SaveChangesAsync();
    }

    [Fact]
    public async Task Handle_WhenDescriptionHasNoSlugId_ReturnsWithoutSavingAndDoesNotCallRepository()
    {
        var request = new HandleSepayWebhookRequest
        {
            TransactionId = "TXN-002",
            Amount = 100_000,
            Description = "Chuyen khoan tien hang thang",
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
            TransactionId = "TXN-003",
            Amount = 50_000,
            Description = "Payment ORD-MISSING123 ref",
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

        _orderRepository
            .FindByExpressionAsync(Arg.Any<FilterSpecification<Domain.Order.Order>>())
            .Returns(order);

        var request = new HandleSepayWebhookRequest
        {
            TransactionId = "TXN-004",
            Amount = order.TotalAmount,
            Description = $"Duplicate webhook {order.SlugId}",
            TransferredAt = DateTime.UtcNow,
        };

        await _handler.Handle(request, CancellationToken.None);

        await _orderRepository.DidNotReceive().SaveChangesAsync();
        order.PaidAt.Should().NotBe(request.TransferredAt);
    }

    [Fact]
    public async Task Handle_WhenAmountDoesNotMatchOrderTotal_DoesNotMarkAsPaidAndDoesNotSave()
    {
        var userId = Guid.NewGuid();
        var order = Domain.Order.Order.Create(userId, PaymentMethod.Banking, new List<Guid>());

        _orderRepository
            .FindByExpressionAsync(Arg.Any<FilterSpecification<Domain.Order.Order>>())
            .Returns(order);

        var request = new HandleSepayWebhookRequest
        {
            TransactionId = "TXN-005",
            Amount = order.TotalAmount + 1,
            Description = $"Payment {order.SlugId}",
            TransferredAt = DateTime.UtcNow,
        };

        await _handler.Handle(request, CancellationToken.None);

        order.PaymentStatus.Should().Be(PaymentStatus.Unpaid);
        await _orderRepository.DidNotReceive().SaveChangesAsync();
    }

    [Fact]
    public async Task Handle_WhenDescriptionContainsPrefixStrippedSlugBody_PrependsOrdAndMarksOrderAsPaid()
    {
        var userId = Guid.NewGuid();
        var order = Domain.Order.Order.Create(userId, PaymentMethod.Banking, new List<Guid>());
        var slugBody = order.SlugId[4..]; // strips "ORD-"
        var transferredAt = new DateTime(2026, 4, 5, 10, 0, 0, DateTimeKind.Utc);

        _orderRepository
            .FindByExpressionAsync(Arg.Any<FilterSpecification<Domain.Order.Order>>())
            .Returns(order);

        var request = new HandleSepayWebhookRequest
        {
            TransactionId = "TXN-006",
            Amount = order.TotalAmount,
            Description = $"Chuyen khoan {slugBody} thanh toan",
            TransferredAt = transferredAt,
        };

        await _handler.Handle(request, CancellationToken.None);

        order.PaymentStatus.Should().Be(PaymentStatus.Paid);
        await _orderRepository.Received(1).SaveChangesAsync();
    }
}
