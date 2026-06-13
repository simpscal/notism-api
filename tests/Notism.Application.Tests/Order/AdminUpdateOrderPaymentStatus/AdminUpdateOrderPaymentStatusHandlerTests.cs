using FluentAssertions;

using Microsoft.Extensions.Logging;

using Notism.Application.Common.Services;
using Notism.Application.Order.AdminUpdateOrderPaymentStatus;
using Notism.Application.Tests.Common;
using Notism.Domain.Payment.Enums;
using Notism.Domain.User.Enums;
using Notism.Infrastructure.Repositories;
using Notism.Shared.Exceptions;

using NSubstitute;

using DomainOrder = Notism.Domain.Order.Order;
using PaymentMethodEnum = Notism.Domain.Order.Enums.PaymentMethod;

namespace Notism.Application.Tests.Order.AdminUpdateOrderPaymentStatus;

public class AdminUpdateOrderPaymentStatusHandlerTests : IDisposable
{
    private readonly WriteHandlerContext _context;
    private readonly IMessages _messages;
    private readonly AdminUpdateOrderPaymentStatusHandler _handler;
    private Guid _userId;

    public AdminUpdateOrderPaymentStatusHandlerTests()
    {
        _context = new WriteHandlerContext();
        _messages = Substitute.For<IMessages>();
        _messages.OrderNotFound.Returns("Order not found.");

        _handler = new AdminUpdateOrderPaymentStatusHandler(
            new OrderRepository(_context.DbContext),
            _context.DbContext,
            Substitute.For<ILogger<AdminUpdateOrderPaymentStatusHandler>>(),
            _messages);
    }

    [Fact]
    public async Task Handle_WhenTargetIsPaid_PersistsPaidStatusAndPaidAt()
    {
        var order = await SeedOrderAsync(PaymentMethodEnum.Banking);

        var result = await _handler.Handle(
            new AdminUpdateOrderPaymentStatusRequest { OrderId = order.Id, PaymentStatus = "paid" },
            CancellationToken.None);

        result.PaymentStatus.Should().Be("paid");
        result.PaidAt.Should().NotBeNull();

        var persisted = await GetPersistedAsync(order.Id);
        persisted.PaymentStatus.Should().Be(PaymentStatus.Paid);
        persisted.PaidAt.Should().NotBeNull();
    }

    [Fact]
    public async Task Handle_WhenTargetIsFailed_PersistsFailedStatus()
    {
        var order = await SeedOrderAsync(PaymentMethodEnum.Banking);

        var result = await _handler.Handle(
            new AdminUpdateOrderPaymentStatusRequest { OrderId = order.Id, PaymentStatus = "failed" },
            CancellationToken.None);

        result.PaymentStatus.Should().Be("failed");

        var persisted = await GetPersistedAsync(order.Id);
        persisted.PaymentStatus.Should().Be(PaymentStatus.Failed);
    }

    [Fact]
    public async Task Handle_WhenTargetIsUnpaidAfterPaid_RevertsToUnpaidAndClearsPaidAt()
    {
        var order = DomainOrder.Create(_userId == Guid.Empty ? await NewUserAsync() : _userId, PaymentMethodEnum.Banking, new List<Guid>());
        order.MarkAsPaid(DateTime.UtcNow);
        await PersistSeedAsync(order);

        var result = await _handler.Handle(
            new AdminUpdateOrderPaymentStatusRequest { OrderId = order.Id, PaymentStatus = "unpaid" },
            CancellationToken.None);

        result.PaymentStatus.Should().Be("unpaid");
        result.PaidAt.Should().BeNull();

        var persisted = await GetPersistedAsync(order.Id);
        persisted.PaymentStatus.Should().Be(PaymentStatus.Unpaid);
        persisted.PaidAt.Should().BeNull();
    }

    [Fact]
    public async Task Handle_WhenPaymentMethodIsCashOnDelivery_AllowsTransitionRegardlessOfMethod()
    {
        var order = await SeedOrderAsync(PaymentMethodEnum.CashOnDelivery);

        var result = await _handler.Handle(
            new AdminUpdateOrderPaymentStatusRequest { OrderId = order.Id, PaymentStatus = "paid" },
            CancellationToken.None);

        result.PaymentStatus.Should().Be("paid");
        result.PaidAt.Should().NotBeNull();
    }

    [Fact]
    public async Task Handle_WhenTargetEqualsCurrent_IsNoOpAndPersistsUnchanged()
    {
        var order = await SeedOrderAsync(PaymentMethodEnum.Banking);

        var result = await _handler.Handle(
            new AdminUpdateOrderPaymentStatusRequest { OrderId = order.Id, PaymentStatus = "unpaid" },
            CancellationToken.None);

        result.PaymentStatus.Should().Be("unpaid");
        result.PaidAt.Should().BeNull();

        var persisted = await GetPersistedAsync(order.Id);
        persisted.PaymentStatus.Should().Be(PaymentStatus.Unpaid);
    }

    [Fact]
    public async Task Handle_WhenOrderNotFound_ThrowsResultFailureException()
    {
        var act = async () => await _handler.Handle(
            new AdminUpdateOrderPaymentStatusRequest { OrderId = Guid.NewGuid(), PaymentStatus = "paid" },
            CancellationToken.None);

        await act.Should().ThrowAsync<ResultFailureException>();
    }

    public void Dispose()
        => _context.Dispose();

    private async Task<DomainOrder> SeedOrderAsync(PaymentMethodEnum paymentMethod)
    {
        var userId = await NewUserAsync();
        var order = DomainOrder.Create(userId, paymentMethod, new List<Guid>());
        await PersistSeedAsync(order);
        return order;
    }

    private async Task PersistSeedAsync(DomainOrder order)
    {
        order.ClearDomainEvents();
        await _context.SeedAsync(order);
    }

    private async Task<Guid> NewUserAsync()
    {
        var user = Domain.User.User.Create($"admin-payment-{Guid.NewGuid():N}@example.com", "hashedpassword", UserRole.User);
        user.ClearDomainEvents();
        await _context.SeedAsync(user);
        _userId = user.Id;
        return user.Id;
    }

    private async Task<DomainOrder> GetPersistedAsync(Guid orderId)
    {
        _context.DbContext.ChangeTracker.Clear();
        return await _context.DbContext.Orders.FindAsync(orderId)
            ?? throw new InvalidOperationException("Order was not persisted.");
    }
}