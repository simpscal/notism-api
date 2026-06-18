using FluentAssertions;

using Microsoft.Extensions.Logging;

using Notism.Application.Common.Services;
using Notism.Application.Order.GetOrders;
using Notism.Application.Tests.Common;
using Notism.Domain.Order.Enums;
using Notism.Domain.User.Enums;
using Notism.Infrastructure.Persistence;

using NSubstitute;

using DomainOrder = Notism.Domain.Order.Order;

namespace Notism.Application.Tests.Order.GetOrders;

public class GetOrdersHandlerTests
{
    private readonly AppDbContext _dbContext;
    private readonly GetOrdersHandler _handler;
    private Guid _userId;

    public GetOrdersHandlerTests()
    {
        _dbContext = ReadDbContextFactory.Create();
        _handler = new GetOrdersHandler(
            _dbContext,
            Substitute.For<IStorageService>(),
            Substitute.For<ILogger<GetOrdersHandler>>());
    }

    [Fact]
    public async Task Handle_WithFirstBatch_ReturnsPagedItemsAndTotalCount()
    {
        await SeedUserAsync();
        await SeedOrdersAsync(
            NewOrder(),
            NewOrder(),
            NewOrder(),
            NewOrder(),
            NewOrder());

        var result = await _handler.Handle(
            new GetOrdersRequest { UserId = _userId, Skip = 0, Take = 2 },
            CancellationToken.None);

        result.Items.Should().HaveCount(2);
        result.TotalCount.Should().Be(5);
    }

    [Fact]
    public async Task Handle_WhenUserHasNoOrders_ReturnsEmptyPageWithZeroTotal()
    {
        await SeedUserAsync();

        var result = await _handler.Handle(
            new GetOrdersRequest { UserId = _userId },
            CancellationToken.None);

        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
    }

    [Fact]
    public async Task Handle_ExcludesCancelledOrders()
    {
        await SeedUserAsync();
        var cancelled = NewOrder();
        cancelled.Cancel();
        await SeedOrdersAsync(NewOrder(), cancelled);

        var result = await _handler.Handle(
            new GetOrdersRequest { UserId = _userId },
            CancellationToken.None);

        result.TotalCount.Should().Be(1);
    }

    [Fact]
    public async Task Handle_WithPaidPaymentStatusFilter_ReturnsOnlyPaidOrders()
    {
        await SeedUserAsync();
        var paid = NewOrder();
        paid.MarkAsPaid(DateTime.UtcNow);
        await SeedOrdersAsync(paid, NewOrder());

        var result = await _handler.Handle(
            new GetOrdersRequest { UserId = _userId, PaymentStatus = "paid" },
            CancellationToken.None);

        result.TotalCount.Should().Be(1);
        result.Items.Single().PaymentStatus.Should().Be("paid");
    }

    [Fact]
    public async Task Handle_WithUnrecognisedPaymentStatusFilter_DoesNotFilterByPaymentStatus()
    {
        await SeedUserAsync();
        var paid = NewOrder();
        paid.MarkAsPaid(DateTime.UtcNow);
        await SeedOrdersAsync(paid, NewOrder());

        var result = await _handler.Handle(
            new GetOrdersRequest { UserId = _userId, PaymentStatus = "unknown-value" },
            CancellationToken.None);

        result.TotalCount.Should().Be(2);
    }

    [Fact]
    public async Task Handle_OrdersByCreatedAtDescendingThenIdDescending()
    {
        await SeedUserAsync();

        var shared = new DateTime(2026, 5, 1, 12, 0, 0, DateTimeKind.Utc);
        var earlier = NewOrderWith(new DateTime(2026, 4, 1, 12, 0, 0, DateTimeKind.Utc), Guid.Parse("00000000-0000-0000-0000-000000000001"));
        var tieLowerId = NewOrderWith(shared, Guid.Parse("11111111-1111-1111-1111-111111111111"));
        var tieHigherId = NewOrderWith(shared, Guid.Parse("22222222-2222-2222-2222-222222222222"));
        await SeedOrdersAsync(earlier, tieLowerId, tieHigherId);

        var result = await _handler.Handle(
            new GetOrdersRequest { UserId = _userId, Take = 10 },
            CancellationToken.None);

        result.Items.Select(i => i.Id).Should().ContainInOrder(tieHigherId.Id, tieLowerId.Id, earlier.Id);
    }

    [Fact]
    public async Task Handle_WhenOrderIsPaid_ResponseIncludesPaymentStatusPaidAndPaidAt()
    {
        await SeedUserAsync();
        var paidAt = new DateTime(2026, 4, 5, 9, 0, 0, DateTimeKind.Utc);
        var order = NewOrder();
        order.MarkAsPaid(paidAt);
        await SeedOrdersAsync(order);

        var result = await _handler.Handle(
            new GetOrdersRequest { UserId = _userId },
            CancellationToken.None);

        var orderResponse = result.Items.Single();
        orderResponse.PaymentStatus.Should().Be("paid");
        orderResponse.PaidAt.Should().Be(paidAt);
    }

    [Fact]
    public async Task Handle_WhenOrderIsUnpaid_ResponseIncludesPaymentStatusUnpaidAndNullPaidAt()
    {
        await SeedUserAsync();
        await SeedOrdersAsync(NewOrder());

        var result = await _handler.Handle(
            new GetOrdersRequest { UserId = _userId },
            CancellationToken.None);

        var orderResponse = result.Items.Single();
        orderResponse.PaymentStatus.Should().Be("unpaid");
        orderResponse.PaidAt.Should().BeNull();
    }

    [Fact]
    public async Task Handle_WhenOrderHasPendingRefund_ListItemCarriesPendingRefund()
    {
        await SeedUserAsync();
        var order = RefundableOrder();
        order.CreateRefund();
        await SeedOrdersAsync(order);

        var result = await _handler.Handle(
            new GetOrdersRequest { UserId = _userId },
            CancellationToken.None);

        var orderResponse = result.Items.Single();
        orderResponse.Refund.Should().NotBeNull();
        orderResponse.Refund!.Status.Should().Be("pending");
        orderResponse.Refund.Amount.Should().Be(order.TotalAmount);
    }

    [Fact]
    public async Task Handle_WhenRefundPaid_ListItemCarriesPaidRefundWithTransferReferenceAndSentDate()
    {
        await SeedUserAsync();
        var order = RefundableOrder();
        order.CreateRefund();
        order.MarkRefundProcessing();
        order.MarkRefundPaid("SEPAY-TX-PAID");
        await SeedOrdersAsync(order);

        var result = await _handler.Handle(
            new GetOrdersRequest { UserId = _userId },
            CancellationToken.None);

        var orderResponse = result.Items.Single();
        orderResponse.Refund.Should().NotBeNull();
        orderResponse.Refund!.Status.Should().Be("paid");
        orderResponse.Refund.TransferReference.Should().Be("SEPAY-TX-PAID");
        orderResponse.Refund.SentDate.Should().NotBeNull();
    }

    [Fact]
    public async Task Handle_WhenRefundProcessing_ListItemCollapsesStatusToPending()
    {
        await SeedUserAsync();
        var order = RefundableOrder();
        order.CreateRefund();
        order.MarkRefundProcessing();
        await SeedOrdersAsync(order);

        var result = await _handler.Handle(
            new GetOrdersRequest { UserId = _userId },
            CancellationToken.None);

        var orderResponse = result.Items.Single();
        orderResponse.Refund.Should().NotBeNull();
        orderResponse.Refund!.Status.Should().Be("pending");
    }

    [Fact]
    public async Task Handle_WhenRefundFailed_ListItemCollapsesStatusToPending()
    {
        await SeedUserAsync();
        var order = RefundableOrder();
        order.CreateRefund();
        order.MarkRefundProcessing();
        order.MarkRefundFailed("provider declined");
        await SeedOrdersAsync(order);

        var result = await _handler.Handle(
            new GetOrdersRequest { UserId = _userId },
            CancellationToken.None);

        var orderResponse = result.Items.Single();
        orderResponse.Refund.Should().NotBeNull();
        orderResponse.Refund!.Status.Should().Be("pending");
        orderResponse.Refund.TransferReference.Should().BeNull();
        orderResponse.Refund.SentDate.Should().BeNull();
    }

    [Fact]
    public async Task Handle_WhenOrderHasNoRefund_ListItemCarriesNullRefund()
    {
        await SeedUserAsync();
        await SeedOrdersAsync(NewOrder());

        var result = await _handler.Handle(
            new GetOrdersRequest { UserId = _userId },
            CancellationToken.None);

        result.Items.Single().Refund.Should().BeNull();
    }

    private DomainOrder NewOrder()
        => DomainOrder.Create(_userId, PaymentMethod.Banking, new List<Guid>());

    private DomainOrder RefundableOrder()
    {
        var order = NewOrder();
        order.AddItem(Domain.Order.OrderItem.Create(order.Id, Guid.NewGuid(), "Burger", unitPrice: 150_000m, discountPrice: null, quantity: 1));
        order.MarkAsPaid(DateTime.UtcNow);
        return order;
    }

    private DomainOrder NewOrderWith(DateTime createdAt, Guid id)
    {
        var order = NewOrder();
        typeof(DomainOrder).GetProperty(nameof(DomainOrder.Id))!.SetValue(order, id);
        order.CreatedAt = createdAt;
        return order;
    }

    private async Task SeedUserAsync()
    {
        var user = Domain.User.User.Create("orders@example.com", "hashedpassword", UserRole.User);
        user.ClearDomainEvents();
        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();
        _userId = user.Id;
        _dbContext.ChangeTracker.Clear();
    }

    private async Task SeedOrdersAsync(params DomainOrder[] orders)
    {
        foreach (var order in orders)
        {
            order.ClearDomainEvents();
            _dbContext.Orders.Add(order);
        }

        await _dbContext.SaveChangesAsync();
        _dbContext.ChangeTracker.Clear();
    }
}