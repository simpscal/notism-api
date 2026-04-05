using FluentAssertions;

using Microsoft.Extensions.Logging;

using Notism.Application.Common.Interfaces;
using Notism.Application.Order.GetOrders;
using Notism.Domain.Common.Specifications;
using Notism.Domain.Order;
using Notism.Domain.Order.Enums;
using Notism.Domain.Payment.Enums;

using NSubstitute;

namespace Notism.Application.Tests.Order.GetOrders;

public class GetOrdersHandlerTests
{
    private readonly IOrderRepository _orderRepository;
    private readonly IStorageService _storageService;
    private readonly ILogger<GetOrdersHandler> _logger;
    private readonly GetOrdersHandler _handler;

    public GetOrdersHandlerTests()
    {
        _orderRepository = Substitute.For<IOrderRepository>();
        _storageService = Substitute.For<IStorageService>();
        _logger = Substitute.For<ILogger<GetOrdersHandler>>();
        _handler = new GetOrdersHandler(_orderRepository, _storageService, _logger);
    }

    [Fact]
    public async Task Handle_WithNoPaymentStatusFilter_ReturnsAllNonCancelledOrders()
    {
        var userId = Guid.NewGuid();
        var paidOrder = Domain.Order.Order.Create(userId, PaymentMethod.Banking, new List<Guid>());
        paidOrder.MarkAsPaid(DateTime.UtcNow);
        var unpaidOrder = Domain.Order.Order.Create(userId, PaymentMethod.Banking, new List<Guid>());

        _orderRepository
            .FilterByExpressionAsync(Arg.Any<FilterSpecification<Domain.Order.Order>>())
            .Returns(new List<Domain.Order.Order> { paidOrder, unpaidOrder });

        var request = new GetOrdersRequest { UserId = userId };
        var result = await _handler.Handle(request, CancellationToken.None);

        result.Orders.Should().HaveCount(2);
    }

    [Fact]
    public async Task Handle_WithPaidPaymentStatusFilter_ReturnsOnlyPaidOrders()
    {
        var userId = Guid.NewGuid();
        var paidOrder = Domain.Order.Order.Create(userId, PaymentMethod.Banking, new List<Guid>());
        paidOrder.MarkAsPaid(DateTime.UtcNow);
        var unpaidOrder = Domain.Order.Order.Create(userId, PaymentMethod.Banking, new List<Guid>());

        _orderRepository
            .FilterByExpressionAsync(Arg.Any<FilterSpecification<Domain.Order.Order>>())
            .Returns(new List<Domain.Order.Order> { paidOrder, unpaidOrder });

        var request = new GetOrdersRequest { UserId = userId, PaymentStatus = "paid" };
        var result = await _handler.Handle(request, CancellationToken.None);

        result.Orders.Should().HaveCount(1);
        result.Orders[0].PaymentStatus.Should().Be("paid");
    }

    [Fact]
    public async Task Handle_WithUnpaidPaymentStatusFilter_ReturnsOnlyUnpaidOrders()
    {
        var userId = Guid.NewGuid();
        var paidOrder = Domain.Order.Order.Create(userId, PaymentMethod.Banking, new List<Guid>());
        paidOrder.MarkAsPaid(DateTime.UtcNow);
        var unpaidOrder = Domain.Order.Order.Create(userId, PaymentMethod.Banking, new List<Guid>());

        _orderRepository
            .FilterByExpressionAsync(Arg.Any<FilterSpecification<Domain.Order.Order>>())
            .Returns(new List<Domain.Order.Order> { paidOrder, unpaidOrder });

        var request = new GetOrdersRequest { UserId = userId, PaymentStatus = "unpaid" };
        var result = await _handler.Handle(request, CancellationToken.None);

        result.Orders.Should().HaveCount(1);
        result.Orders[0].PaymentStatus.Should().Be("unpaid");
    }

    [Fact]
    public async Task Handle_WithUnrecognisedPaymentStatusFilter_ReturnsAllOrders()
    {
        var userId = Guid.NewGuid();
        var paidOrder = Domain.Order.Order.Create(userId, PaymentMethod.Banking, new List<Guid>());
        paidOrder.MarkAsPaid(DateTime.UtcNow);
        var unpaidOrder = Domain.Order.Order.Create(userId, PaymentMethod.Banking, new List<Guid>());

        _orderRepository
            .FilterByExpressionAsync(Arg.Any<FilterSpecification<Domain.Order.Order>>())
            .Returns(new List<Domain.Order.Order> { paidOrder, unpaidOrder });

        var request = new GetOrdersRequest { UserId = userId, PaymentStatus = "unknown-value" };
        var result = await _handler.Handle(request, CancellationToken.None);

        result.Orders.Should().HaveCount(2);
    }

    [Fact]
    public async Task Handle_WhenOrderIsPaid_ResponseIncludesPaymentStatusPaidAndPaidAt()
    {
        var userId = Guid.NewGuid();
        var paidAt = new DateTime(2026, 4, 5, 9, 0, 0, DateTimeKind.Utc);
        var order = Domain.Order.Order.Create(userId, PaymentMethod.Banking, new List<Guid>());
        order.MarkAsPaid(paidAt);

        _orderRepository
            .FilterByExpressionAsync(Arg.Any<FilterSpecification<Domain.Order.Order>>())
            .Returns(new List<Domain.Order.Order> { order });

        var request = new GetOrdersRequest { UserId = userId };
        var result = await _handler.Handle(request, CancellationToken.None);

        var orderResponse = result.Orders.Single();
        orderResponse.PaymentStatus.Should().Be("paid");
        orderResponse.PaidAt.Should().Be(paidAt);
    }

    [Fact]
    public async Task Handle_WhenOrderIsUnpaid_ResponseIncludesPaymentStatusUnpaidAndNullPaidAt()
    {
        var userId = Guid.NewGuid();
        var order = Domain.Order.Order.Create(userId, PaymentMethod.Banking, new List<Guid>());

        _orderRepository
            .FilterByExpressionAsync(Arg.Any<FilterSpecification<Domain.Order.Order>>())
            .Returns(new List<Domain.Order.Order> { order });

        var request = new GetOrdersRequest { UserId = userId };
        var result = await _handler.Handle(request, CancellationToken.None);

        var orderResponse = result.Orders.Single();
        orderResponse.PaymentStatus.Should().Be("unpaid");
        orderResponse.PaidAt.Should().BeNull();
    }
}
