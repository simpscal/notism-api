using FluentAssertions;

using Microsoft.Extensions.Logging;

using Notism.Application.Order.AdminOrdersForTable;
using Notism.Domain.Common.Interfaces;
using Notism.Domain.Order;
using Notism.Domain.Order.Enums;
using Notism.Shared.Models;

using NSubstitute;

namespace Notism.Application.Tests.Order.AdminOrdersForTable;

public class AdminOrdersForTableHandlerTests
{
    private readonly IOrderRepository _orderRepository;
    private readonly ILogger<AdminOrdersForTableHandler> _logger;
    private readonly AdminOrdersForTableHandler _handler;

    public AdminOrdersForTableHandlerTests()
    {
        _orderRepository = Substitute.For<IOrderRepository>();
        _logger = Substitute.For<ILogger<AdminOrdersForTableHandler>>();
        _handler = new AdminOrdersForTableHandler(_orderRepository, _logger);
    }

    [Fact]
    public async Task Handle_WhenOrderIsUnpaid_MapsPaymentStatusUnpaidAndNullPaidAt()
    {
        var order = Domain.Order.Order.Create(Guid.NewGuid(), PaymentMethod.Banking, new List<Guid>());
        StubPagedResult(order);

        var result = await _handler.Handle(new AdminOrdersForTableRequest(), CancellationToken.None);

        var item = result.Items.Single();
        item.PaymentStatus.Should().Be("unpaid");
        item.PaidAt.Should().BeNull();
    }

    [Fact]
    public async Task Handle_WhenOrderIsPaid_MapsPaymentStatusPaidAndPaidAt()
    {
        var paidAt = new DateTime(2026, 4, 5, 8, 30, 0, DateTimeKind.Utc);
        var order = Domain.Order.Order.Create(Guid.NewGuid(), PaymentMethod.Banking, new List<Guid>());
        order.MarkAsPaid(paidAt);
        StubPagedResult(order);

        var result = await _handler.Handle(new AdminOrdersForTableRequest(), CancellationToken.None);

        var item = result.Items.Single();
        item.PaymentStatus.Should().Be("paid");
        item.PaidAt.Should().Be(paidAt);
    }

    [Fact]
    public async Task Handle_WhenUserNotLoaded_MapsEmptyEmailAndName()
    {
        var order = Domain.Order.Order.Create(Guid.NewGuid(), PaymentMethod.CashOnDelivery, new List<Guid>());
        StubPagedResult(order);

        var result = await _handler.Handle(new AdminOrdersForTableRequest(), CancellationToken.None);

        var item = result.Items.Single();
        item.UserEmail.Should().BeEmpty();
        item.UserName.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_MapsDeliveryStatusAsStringValue()
    {
        var order = Domain.Order.Order.Create(Guid.NewGuid(), PaymentMethod.Banking, new List<Guid>());
        StubPagedResult(order);

        var result = await _handler.Handle(new AdminOrdersForTableRequest(), CancellationToken.None);

        result.Items.Single().DeliveryStatus.Should().NotBeNullOrEmpty();
    }

    private void StubPagedResult(params Domain.Order.Order[] orders)
    {
        _orderRepository
            .FilterPagedByExpressionAsync(Arg.Any<ISpecification<Domain.Order.Order>>(), Arg.Any<Pagination>())
            .Returns(new PagedResult<Domain.Order.Order>
            {
                TotalCount = orders.Length,
                Items = orders,
            });
    }
}