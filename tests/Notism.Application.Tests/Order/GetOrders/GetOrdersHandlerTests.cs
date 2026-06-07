using FluentAssertions;

using Microsoft.Extensions.Logging;

using Notism.Application.Common.Interfaces;
using Notism.Application.Order.Common;
using Notism.Application.Order.GetOrders;
using Notism.Domain.Common.Interfaces;
using Notism.Domain.Order;
using Notism.Domain.Order.Enums;
using Notism.Domain.Payment.Enums;
using Notism.Shared.Models;

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
    public async Task Handle_WithFirstBatch_ReturnsPagedItemsAndTotalCount()
    {
        var userId = Guid.NewGuid();
        var firstOrder = Domain.Order.Order.Create(userId, PaymentMethod.Banking, new List<Guid>());
        var secondOrder = Domain.Order.Order.Create(userId, PaymentMethod.Banking, new List<Guid>());

        SetupPagedResult(5, firstOrder, secondOrder);

        var request = new GetOrdersRequest { UserId = userId, Skip = 0, Take = 2 };
        var result = await _handler.Handle(request, CancellationToken.None);

        result.Items.Should().HaveCount(2);
        result.TotalCount.Should().Be(5);
    }

    [Fact]
    public async Task Handle_WhenUserHasNoOrders_ReturnsEmptyPageWithZeroTotal()
    {
        var userId = Guid.NewGuid();
        SetupPagedResult(0);

        var request = new GetOrdersRequest { UserId = userId };
        var result = await _handler.Handle(request, CancellationToken.None);

        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
    }

    [Fact]
    public async Task Handle_PassesUserPaginationToRepository()
    {
        var userId = Guid.NewGuid();
        SetupPagedResult(0);

        var request = new GetOrdersRequest { UserId = userId, Skip = 25, Take = 25 };
        await _handler.Handle(request, CancellationToken.None);

        await _orderRepository.Received(1).FilterPagedByExpressionAsync(
            Arg.Any<ISpecification<Domain.Order.Order>>(),
            Arg.Is<Pagination>(p => p.Skip == 25 && p.Take == 25));
    }

    [Fact]
    public async Task Handle_WithPaidPaymentStatusFilter_PassesPaymentStatusToSpecification()
    {
        var userId = Guid.NewGuid();
        SetupPagedResult(0);

        OrderDetailSpecification? capturedSpecification = null;
        _orderRepository
            .FilterPagedByExpressionAsync(
                Arg.Do<ISpecification<Domain.Order.Order>>(s => capturedSpecification = s as OrderDetailSpecification),
                Arg.Any<Pagination>())
            .Returns(new PagedResult<Domain.Order.Order> { TotalCount = 0, Items = new List<Domain.Order.Order>() });

        var paidOrder = Domain.Order.Order.Create(userId, PaymentMethod.Banking, new List<Guid>());
        paidOrder.MarkAsPaid(DateTime.UtcNow);
        var unpaidOrder = Domain.Order.Order.Create(userId, PaymentMethod.Banking, new List<Guid>());

        var request = new GetOrdersRequest { UserId = userId, PaymentStatus = "paid" };
        await _handler.Handle(request, CancellationToken.None);

        capturedSpecification.Should().NotBeNull();
        capturedSpecification!.IsSatisfiedBy(paidOrder).Should().BeTrue();
        capturedSpecification.IsSatisfiedBy(unpaidOrder).Should().BeFalse();
    }

    [Fact]
    public async Task Handle_WithUnrecognisedPaymentStatusFilter_DoesNotFilterByPaymentStatus()
    {
        var userId = Guid.NewGuid();
        SetupPagedResult(0);

        OrderDetailSpecification? capturedSpecification = null;
        _orderRepository
            .FilterPagedByExpressionAsync(
                Arg.Do<ISpecification<Domain.Order.Order>>(s => capturedSpecification = s as OrderDetailSpecification),
                Arg.Any<Pagination>())
            .Returns(new PagedResult<Domain.Order.Order> { TotalCount = 0, Items = new List<Domain.Order.Order>() });

        var paidOrder = Domain.Order.Order.Create(userId, PaymentMethod.Banking, new List<Guid>());
        paidOrder.MarkAsPaid(DateTime.UtcNow);
        var unpaidOrder = Domain.Order.Order.Create(userId, PaymentMethod.Banking, new List<Guid>());

        var request = new GetOrdersRequest { UserId = userId, PaymentStatus = "unknown-value" };
        await _handler.Handle(request, CancellationToken.None);

        capturedSpecification.Should().NotBeNull();
        capturedSpecification!.IsSatisfiedBy(paidOrder).Should().BeTrue();
        capturedSpecification.IsSatisfiedBy(unpaidOrder).Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WhenOrderIsPaid_ResponseIncludesPaymentStatusPaidAndPaidAt()
    {
        var userId = Guid.NewGuid();
        var paidAt = new DateTime(2026, 4, 5, 9, 0, 0, DateTimeKind.Utc);
        var order = Domain.Order.Order.Create(userId, PaymentMethod.Banking, new List<Guid>());
        order.MarkAsPaid(paidAt);

        SetupPagedResult(1, order);

        var request = new GetOrdersRequest { UserId = userId };
        var result = await _handler.Handle(request, CancellationToken.None);

        var orderResponse = result.Items.Single();
        orderResponse.PaymentStatus.Should().Be("paid");
        orderResponse.PaidAt.Should().Be(paidAt);
    }

    [Fact]
    public async Task Handle_WhenOrderIsUnpaid_ResponseIncludesPaymentStatusUnpaidAndNullPaidAt()
    {
        var userId = Guid.NewGuid();
        var order = Domain.Order.Order.Create(userId, PaymentMethod.Banking, new List<Guid>());

        SetupPagedResult(1, order);

        var request = new GetOrdersRequest { UserId = userId };
        var result = await _handler.Handle(request, CancellationToken.None);

        var orderResponse = result.Items.Single();
        orderResponse.PaymentStatus.Should().Be("unpaid");
        orderResponse.PaidAt.Should().BeNull();
    }

    private void SetupPagedResult(int totalCount, params Domain.Order.Order[] orders)
    {
        _orderRepository
            .FilterPagedByExpressionAsync(
                Arg.Any<ISpecification<Domain.Order.Order>>(),
                Arg.Any<Pagination>())
            .Returns(new PagedResult<Domain.Order.Order>
            {
                TotalCount = totalCount,
                Items = orders,
            });
    }
}