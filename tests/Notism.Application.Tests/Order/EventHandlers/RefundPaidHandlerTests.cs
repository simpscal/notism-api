using FluentAssertions;

using Microsoft.Extensions.Logging;

using Notism.Application.Common.Services;
using Notism.Application.Order.EventHandlers;
using Notism.Application.Tests.Common;
using Notism.Domain.Order.Events;
using Notism.Domain.User.Enums;
using Notism.Infrastructure.Persistence;

using NSubstitute;

using DomainOrder = Notism.Domain.Order.Order;

namespace Notism.Application.Tests.Order.EventHandlers;

public class RefundPaidHandlerTests
{
    private readonly AppDbContext _dbContext;
    private readonly IPaymentNotifier _paymentNotifier;
    private readonly IEmailService _emailService;
    private readonly RefundPaidHandler _handler;

    public RefundPaidHandlerTests()
    {
        _dbContext = ReadDbContextFactory.Create();
        _paymentNotifier = Substitute.For<IPaymentNotifier>();
        _emailService = Substitute.For<IEmailService>();

        _handler = new RefundPaidHandler(
            _dbContext,
            _paymentNotifier,
            _emailService,
            Substitute.For<ILogger<RefundPaidHandler>>());
    }

    [Fact]
    public async Task Handle_WhenRefundPaid_PushesNotificationToOwningCustomer()
    {
        var (order, userId) = await SeedPaidRefundOrderAsync();
        var notification = BuildEvent(order, userId);

        await _handler.Handle(notification, CancellationToken.None);

        await _paymentNotifier.Received(1).NotifyRefundPaidAsync(
            order.Refund!.Id,
            order.SlugId,
            order.SlugId,
            order.Refund.Amount,
            order.Refund.PaidAt!.Value,
            userId,
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenRefundPaid_NotifiesAdminsRefundStatusPaid()
    {
        var (order, userId) = await SeedPaidRefundOrderAsync();
        var notification = BuildEvent(order, userId);

        await _handler.Handle(notification, CancellationToken.None);

        await _paymentNotifier.Received(1).NotifyAdminRefundStatusChangedAsync(
            order.Refund!.Id,
            "paid",
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenAdminNotifyThrows_StillNotifiesCustomerAndDoesNotRethrow()
    {
        var (order, userId) = await SeedPaidRefundOrderAsync();
        var notification = BuildEvent(order, userId);
        _paymentNotifier
            .NotifyAdminRefundStatusChangedAsync(Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException(new InvalidOperationException("hub down")));

        var act = async () => await _handler.Handle(notification, CancellationToken.None);

        await act.Should().NotThrowAsync();
        await _paymentNotifier.Received(1).NotifyRefundPaidAsync(
            Arg.Any<Guid>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<decimal>(),
            Arg.Any<DateTime>(),
            userId,
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenRefundPaid_SendsConfirmationEmail()
    {
        var (order, userId) = await SeedPaidRefundOrderAsync();
        var notification = BuildEvent(order, userId);

        await _handler.Handle(notification, CancellationToken.None);

        await _emailService.Received(1).SendRefundPaidEmailAsync(
            Arg.Any<Domain.User.ValueObjects.Email>(),
            Arg.Any<string?>(),
            order.SlugId,
            order.Refund!.Amount,
            "SEPAY-TX-PAID",
            order.Refund.PaidAt!.Value,
            order.SlugId);
    }

    [Fact]
    public async Task Handle_WhenEmailThrows_StillPushesNotificationAndDoesNotRethrow()
    {
        var (order, userId) = await SeedPaidRefundOrderAsync();
        var notification = BuildEvent(order, userId);
        _emailService
            .SendRefundPaidEmailAsync(
                Arg.Any<Domain.User.ValueObjects.Email>(),
                Arg.Any<string?>(),
                Arg.Any<string>(),
                Arg.Any<decimal>(),
                Arg.Any<string>(),
                Arg.Any<DateTime>(),
                Arg.Any<string>())
            .Returns(Task.FromException(new InvalidOperationException("mail down")));

        var act = async () => await _handler.Handle(notification, CancellationToken.None);

        await act.Should().NotThrowAsync();
        await _paymentNotifier.Received(1).NotifyRefundPaidAsync(
            Arg.Any<Guid>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<decimal>(),
            Arg.Any<DateTime>(),
            userId,
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenNotificationThrows_StillSendsEmailAndDoesNotRethrow()
    {
        var (order, userId) = await SeedPaidRefundOrderAsync();
        var notification = BuildEvent(order, userId);
        _paymentNotifier
            .NotifyRefundPaidAsync(
                Arg.Any<Guid>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<decimal>(),
                Arg.Any<DateTime>(),
                Arg.Any<Guid>(),
                Arg.Any<CancellationToken>())
            .Returns(Task.FromException(new InvalidOperationException("hub down")));

        var act = async () => await _handler.Handle(notification, CancellationToken.None);

        await act.Should().NotThrowAsync();
        await _emailService.Received(1).SendRefundPaidEmailAsync(
            Arg.Any<Domain.User.ValueObjects.Email>(),
            Arg.Any<string?>(),
            Arg.Any<string>(),
            Arg.Any<decimal>(),
            Arg.Any<string>(),
            Arg.Any<DateTime>(),
            Arg.Any<string>());
    }

    [Fact]
    public async Task Handle_WhenOrderNotFound_DoesNotNotifyOrEmailAndDoesNotThrow()
    {
        var notification = new RefundPaidEvent(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), DateTime.UtcNow, "SEPAY-TX-PAID");

        var act = async () => await _handler.Handle(notification, CancellationToken.None);

        await act.Should().NotThrowAsync();
        await _paymentNotifier.DidNotReceive().NotifyRefundPaidAsync(
            Arg.Any<Guid>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<decimal>(),
            Arg.Any<DateTime>(),
            Arg.Any<Guid>(),
            Arg.Any<CancellationToken>());
        await _emailService.DidNotReceive().SendRefundPaidEmailAsync(
            Arg.Any<Domain.User.ValueObjects.Email>(),
            Arg.Any<string?>(),
            Arg.Any<string>(),
            Arg.Any<decimal>(),
            Arg.Any<string>(),
            Arg.Any<DateTime>(),
            Arg.Any<string>());
    }

    private static RefundPaidEvent BuildEvent(DomainOrder order, Guid userId)
        => new(order.Id, order.Refund!.Id, userId, order.Refund.PaidAt!.Value, "SEPAY-TX-PAID");

    private async Task<(DomainOrder Order, Guid UserId)> SeedPaidRefundOrderAsync()
    {
        var user = Domain.User.User.Create("refundpaid@example.com", "hashedpassword", UserRole.User, "Jane", "Doe");
        user.ClearDomainEvents();
        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();

        var order = DomainOrder.Create(user.Id, Domain.Order.Enums.PaymentMethod.Banking, new List<Guid>());
        order.AddItem(Domain.Order.OrderItem.Create(order.Id, Guid.NewGuid(), "Burger", unitPrice: 150_000m, discountPrice: null, quantity: 1));
        order.MarkAsPaid(DateTime.UtcNow);
        order.CreateRefund();
        order.MarkRefundProcessing();
        order.MarkRefundPaid("SEPAY-TX-PAID");
        order.ClearDomainEvents();
        _dbContext.Orders.Add(order);
        await _dbContext.SaveChangesAsync();
        _dbContext.ChangeTracker.Clear();

        return (order, user.Id);
    }
}