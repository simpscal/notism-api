using FluentAssertions;

using Microsoft.Extensions.Logging;

using Notism.Application.Order.AdminOrdersForTable;
using Notism.Application.Tests.Common;
using Notism.Domain.Order.Enums;
using Notism.Domain.User.Enums;
using Notism.Infrastructure.Persistence;

using NSubstitute;

using DomainOrder = Notism.Domain.Order.Order;

namespace Notism.Application.Tests.Order.AdminOrdersForTable;

public class AdminOrdersForTableHandlerTests
{
    private readonly AppDbContext _dbContext;
    private readonly AdminOrdersForTableHandler _handler;
    private Guid _userId;

    public AdminOrdersForTableHandlerTests()
    {
        _dbContext = ReadDbContextFactory.Create();
        _handler = new AdminOrdersForTableHandler(
            _dbContext,
            Substitute.For<ILogger<AdminOrdersForTableHandler>>());
    }

    [Fact]
    public async Task Handle_WhenOrderIsUnpaid_MapsPaymentStatusUnpaidAndNullPaidAt()
    {
        await SeedUserAsync();
        await SeedOrderAsync(DomainOrder.Create(_userId, PaymentMethod.Banking, new List<Guid>()));

        var result = await _handler.Handle(new AdminOrdersForTableRequest(), CancellationToken.None);

        var item = result.Items.Single();
        item.PaymentStatus.Should().Be("unpaid");
        item.PaidAt.Should().BeNull();
    }

    [Fact]
    public async Task Handle_WhenOrderIsPaid_MapsPaymentStatusPaidAndPaidAt()
    {
        await SeedUserAsync();
        var paidAt = new DateTime(2026, 4, 5, 8, 30, 0, DateTimeKind.Utc);
        var order = DomainOrder.Create(_userId, PaymentMethod.Banking, new List<Guid>());
        order.MarkAsPaid(paidAt);
        await SeedOrderAsync(order);

        var result = await _handler.Handle(new AdminOrdersForTableRequest(), CancellationToken.None);

        var item = result.Items.Single();
        item.PaymentStatus.Should().Be("paid");
        item.PaidAt.Should().Be(paidAt);
    }

    [Fact]
    public async Task Handle_MapsUserEmailAndName()
    {
        await SeedUserAsync("buyer@example.com", "Jane", "Doe");
        await SeedOrderAsync(DomainOrder.Create(_userId, PaymentMethod.CashOnDelivery, new List<Guid>()));

        var result = await _handler.Handle(new AdminOrdersForTableRequest(), CancellationToken.None);

        var item = result.Items.Single();
        item.UserEmail.Should().Be("buyer@example.com");
        item.UserName.Should().Contain("Jane");
    }

    [Fact]
    public async Task Handle_MapsDeliveryStatusAsStringValue()
    {
        await SeedUserAsync();
        await SeedOrderAsync(DomainOrder.Create(_userId, PaymentMethod.Banking, new List<Guid>()));

        var result = await _handler.Handle(new AdminOrdersForTableRequest(), CancellationToken.None);

        result.Items.Single().DeliveryStatus.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Handle_WithKeyword_FiltersBySlug()
    {
        await SeedUserAsync();
        var order = DomainOrder.Create(_userId, PaymentMethod.Banking, new List<Guid>());
        await SeedOrderAsync(order);

        var result = await _handler.Handle(
            new AdminOrdersForTableRequest { Keyword = order.SlugId },
            CancellationToken.None);

        result.Items.Should().ContainSingle(i => i.SlugId == order.SlugId);
    }

    private async Task SeedUserAsync(string email = "table@example.com", string? firstName = null, string? lastName = null)
    {
        var user = Domain.User.User.Create(email, "hashedpassword", UserRole.User, firstName, lastName);
        user.ClearDomainEvents();
        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();
        _userId = user.Id;
        _dbContext.ChangeTracker.Clear();
    }

    private async Task SeedOrderAsync(DomainOrder order)
    {
        order.ClearDomainEvents();
        _dbContext.Orders.Add(order);
        await _dbContext.SaveChangesAsync();
        _dbContext.ChangeTracker.Clear();
    }
}