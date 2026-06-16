using FluentAssertions;

using Microsoft.Extensions.Logging;

using Notism.Application.Order.AdminRefundsForTable;
using Notism.Application.Tests.Common;
using Notism.Domain.Order.Enums;
using Notism.Infrastructure.Persistence;

using NSubstitute;

using DomainOrder = Notism.Domain.Order.Order;
using PaymentMethodEnum = Notism.Domain.Order.Enums.PaymentMethod;

namespace Notism.Application.Tests.Order.AdminRefundsForTable;

public class AdminRefundsForTableHandlerTests
{
    private readonly AppDbContext _dbContext;
    private readonly AdminRefundsForTableHandler _handler;

    public AdminRefundsForTableHandlerTests()
    {
        _dbContext = ReadDbContextFactory.Create();
        _handler = new AdminRefundsForTableHandler(
            _dbContext,
            Substitute.For<ILogger<AdminRefundsForTableHandler>>());
    }

    [Fact]
    public async Task Handle_WhenNoRefunds_ReturnsEmptyPageWithZeroTotal()
    {
        var result = await _handler.Handle(new AdminRefundsForTableRequest(), CancellationToken.None);

        result.TotalCount.Should().Be(0);
        result.Items.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_WhenUnfiltered_ListsAllRefundsIncludingProcessing()
    {
        await SeedOrderWithRefundAsync(RefundStatus.Pending);
        await SeedOrderWithRefundAsync(RefundStatus.Processing);
        await SeedOrderWithRefundAsync(RefundStatus.Paid);
        await SeedOrderWithRefundAsync(RefundStatus.Failed);

        var result = await _handler.Handle(new AdminRefundsForTableRequest(), CancellationToken.None);

        result.TotalCount.Should().Be(4);
        result.Items.Select(i => i.Status)
            .Should().BeEquivalentTo(new[] { "pending", "processing", "paid", "failed" });
    }

    [Fact]
    public async Task Handle_WhenFilteredByPending_ReturnsOnlyPending()
    {
        await SeedOrderWithRefundAsync(RefundStatus.Pending);
        await SeedOrderWithRefundAsync(RefundStatus.Paid);

        var result = await _handler.Handle(
            new AdminRefundsForTableRequest { Status = "pending" },
            CancellationToken.None);

        result.TotalCount.Should().Be(1);
        result.Items.Should().OnlyContain(i => i.Status == "pending");
    }

    [Fact]
    public async Task Handle_WhenFilteredByPaid_ReturnsOnlyPaid()
    {
        await SeedOrderWithRefundAsync(RefundStatus.Pending);
        await SeedOrderWithRefundAsync(RefundStatus.Paid);

        var result = await _handler.Handle(
            new AdminRefundsForTableRequest { Status = "paid" },
            CancellationToken.None);

        result.TotalCount.Should().Be(1);
        result.Items.Should().OnlyContain(i => i.Status == "paid");
    }

    [Fact]
    public async Task Handle_WhenFilteredByFailed_ReturnsOnlyFailed()
    {
        await SeedOrderWithRefundAsync(RefundStatus.Failed);
        await SeedOrderWithRefundAsync(RefundStatus.Paid);

        var result = await _handler.Handle(
            new AdminRefundsForTableRequest { Status = "failed" },
            CancellationToken.None);

        result.TotalCount.Should().Be(1);
        result.Items.Should().OnlyContain(i => i.Status == "failed");
    }

    [Fact]
    public async Task Handle_WhenFilterMatchesNothing_ReturnsEmptyPage()
    {
        await SeedOrderWithRefundAsync(RefundStatus.Pending);

        var result = await _handler.Handle(
            new AdminRefundsForTableRequest { Status = "paid" },
            CancellationToken.None);

        result.TotalCount.Should().Be(0);
        result.Items.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_WhenPaging_ReturnsRequestedSliceWithFullTotal()
    {
        for (var i = 0; i < 5; i++)
        {
            await SeedOrderWithRefundAsync(RefundStatus.Pending);
        }

        var result = await _handler.Handle(
            new AdminRefundsForTableRequest { Skip = 2, Take = 2 },
            CancellationToken.None);

        result.TotalCount.Should().Be(5);
        result.Items.Should().HaveCount(2);
    }

    [Fact]
    public async Task Handle_MapsRefundFieldsAndOrderReference()
    {
        var order = await SeedOrderWithRefundAsync(RefundStatus.Paid);

        var result = await _handler.Handle(new AdminRefundsForTableRequest(), CancellationToken.None);

        var item = result.Items.Single();
        item.Id.Should().Be(order.Refund!.Id);
        item.OrderId.Should().Be(order.Id);
        item.OrderReference.Should().Be(order.SlugId);
        item.Amount.Should().Be(order.Refund.Amount);
        item.Status.Should().Be("paid");
        item.TransferReference.Should().Be(order.Refund.TransferReference);
        item.PaidAt.Should().Be(order.Refund.PaidAt);
        item.CreatedAt.Should().Be(order.Refund.CreatedAt);
    }

    private async Task<DomainOrder> SeedOrderWithRefundAsync(RefundStatus status)
    {
        var order = DomainOrder.Create(Guid.NewGuid(), PaymentMethodEnum.Banking, new List<Guid>());
        order.AddItem(Domain.Order.OrderItem.Create(order.Id, Guid.NewGuid(), "Burger", unitPrice: 150_000m, discountPrice: null, quantity: 1));
        order.MarkAsPaid(DateTime.UtcNow);
        order.CreateRefund();

        switch (status)
        {
            case RefundStatus.Processing:
                order.MarkRefundProcessing();
                break;
            case RefundStatus.Paid:
                order.MarkRefundProcessing();
                order.MarkRefundPaid("TXN-" + Guid.NewGuid().ToString("N")[..8]);
                break;
            case RefundStatus.Failed:
                order.MarkRefundProcessing();
                order.MarkRefundFailed("Bank rejected the transfer");
                break;
        }

        order.ClearDomainEvents();
        _dbContext.Orders.Add(order);
        await _dbContext.SaveChangesAsync();
        _dbContext.ChangeTracker.Clear();
        return order;
    }
}