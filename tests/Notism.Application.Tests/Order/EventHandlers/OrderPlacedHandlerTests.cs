using FluentAssertions;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Notism.Application.Common.Services;
using Notism.Application.Order.EventHandlers;
using Notism.Application.Tests.Common;
using Notism.Domain.Order.Events;
using Notism.Domain.User.Enums;
using Notism.Infrastructure.Persistence;
using Notism.Shared.Configuration;

using NSubstitute;

using DomainOrder = Notism.Domain.Order.Order;

namespace Notism.Application.Tests.Order.EventHandlers;

public class OrderPlacedHandlerTests
{
    private const string OpsRecipient = "ops@example.com";

    private readonly AppDbContext _dbContext;
    private readonly INotifier _paymentNotifier;
    private readonly IEmailService _emailService;
    private readonly OrderPlacedHandler _handler;

    public OrderPlacedHandlerTests()
    {
        _dbContext = ReadDbContextFactory.Create();
        _paymentNotifier = Substitute.For<INotifier>();
        _emailService = Substitute.For<IEmailService>();

        _handler = new OrderPlacedHandler(
            _dbContext,
            _paymentNotifier,
            _emailService,
            Options.Create(new EmailSettings { OpsRecipient = OpsRecipient }),
            Substitute.For<ILogger<OrderPlacedHandler>>());
    }

    [Fact]
    public async Task Handle_WhenOrderPlaced_PushesAdminNotification()
    {
        var (order, _) = await SeedOrderAsync();
        var notification = BuildEvent(order);

        await _handler.Handle(notification, CancellationToken.None);

        await _paymentNotifier.Received(1).NotifyOrderPlacedAsync(
            order.Id,
            order.SlugId,
            order.CreatedAt,
            order.TotalAmount,
            order.Items.Count,
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenOrderPlaced_SendsOpsEmail()
    {
        var (order, _) = await SeedOrderAsync();
        var notification = BuildEvent(order);

        await _handler.Handle(notification, CancellationToken.None);

        await _emailService.Received(1).SendNewOrderEmailAsync(
            OpsRecipient,
            order.SlugId,
            order.CreatedAt,
            order.TotalAmount);
    }

    [Fact]
    public async Task Handle_WhenOrderPlaced_EmailIdentifiesOrderByNumberPlacedAtAndTotal()
    {
        var (order, _) = await SeedOrderAsync();
        var notification = BuildEvent(order);

        await _handler.Handle(notification, CancellationToken.None);

        await _emailService.Received(1).SendNewOrderEmailAsync(
            OpsRecipient,
            order.SlugId,
            order.CreatedAt,
            order.TotalAmount);
        order.SlugId.Should().NotBeNullOrWhiteSpace();
        order.TotalAmount.Should().Be(150_000m);
    }

    [Fact]
    public async Task Handle_WhenNoDashboardConnection_StillSendsOpsEmail()
    {
        var (order, _) = await SeedOrderAsync();
        var notification = BuildEvent(order);
        _paymentNotifier
            .NotifyOrderPlacedAsync(
                Arg.Any<Guid>(),
                Arg.Any<string>(),
                Arg.Any<DateTime>(),
                Arg.Any<decimal>(),
                Arg.Any<int>(),
                Arg.Any<CancellationToken>())
            .Returns(Task.FromException(new InvalidOperationException("no dashboard subscribers")));

        await _handler.Handle(notification, CancellationToken.None);

        await _emailService.Received(1).SendNewOrderEmailAsync(
            OpsRecipient,
            order.SlugId,
            order.CreatedAt,
            order.TotalAmount);
    }

    [Fact]
    public async Task Handle_WhenEmailThrows_StillPushesNotificationAndDoesNotRethrow()
    {
        var (order, _) = await SeedOrderAsync();
        var notification = BuildEvent(order);
        _emailService
            .SendNewOrderEmailAsync(
                Arg.Any<string?>(),
                Arg.Any<string>(),
                Arg.Any<DateTime>(),
                Arg.Any<decimal>())
            .Returns(Task.FromException(new InvalidOperationException("mail down")));

        var act = async () => await _handler.Handle(notification, CancellationToken.None);

        await act.Should().NotThrowAsync();
        await _paymentNotifier.Received(1).NotifyOrderPlacedAsync(
            order.Id,
            Arg.Any<string>(),
            Arg.Any<DateTime>(),
            Arg.Any<decimal>(),
            Arg.Any<int>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenNotificationThrows_StillSendsEmailAndDoesNotRethrow()
    {
        var (order, _) = await SeedOrderAsync();
        var notification = BuildEvent(order);
        _paymentNotifier
            .NotifyOrderPlacedAsync(
                Arg.Any<Guid>(),
                Arg.Any<string>(),
                Arg.Any<DateTime>(),
                Arg.Any<decimal>(),
                Arg.Any<int>(),
                Arg.Any<CancellationToken>())
            .Returns(Task.FromException(new InvalidOperationException("hub down")));

        var act = async () => await _handler.Handle(notification, CancellationToken.None);

        await act.Should().NotThrowAsync();
        await _emailService.Received(1).SendNewOrderEmailAsync(
            OpsRecipient,
            Arg.Any<string>(),
            Arg.Any<DateTime>(),
            Arg.Any<decimal>());
    }

    [Fact]
    public async Task Handle_WhenOrderNotFound_DoesNotNotifyOrEmailAndDoesNotThrow()
    {
        var notification = new OrderCreatedEvent(
            "ORD-NONEXISTENT", Guid.NewGuid(), 100_000m, new List<Guid>());

        var act = async () => await _handler.Handle(notification, CancellationToken.None);

        await act.Should().NotThrowAsync();
        await _paymentNotifier.DidNotReceive().NotifyOrderPlacedAsync(
            Arg.Any<Guid>(),
            Arg.Any<string>(),
            Arg.Any<DateTime>(),
            Arg.Any<decimal>(),
            Arg.Any<int>(),
            Arg.Any<CancellationToken>());
        await _emailService.DidNotReceive().SendNewOrderEmailAsync(
            Arg.Any<string?>(),
            Arg.Any<string>(),
            Arg.Any<DateTime>(),
            Arg.Any<decimal>());
    }

    [Fact]
    public async Task Handle_WithEventRaisedAtConstruction_NotifiesAndEmailsOps()
    {
        var user = Domain.User.User.Create("regression@example.com", "hashedpassword", UserRole.User, "Jane", "Doe");
        user.ClearDomainEvents();
        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();

        var order = DomainOrder.Create(user.Id, Domain.Order.Enums.PaymentMethod.Banking, new List<Guid>());
        var raisedEvent = order.DomainEvents.OfType<OrderCreatedEvent>().Single();
        order.AddItem(Domain.Order.OrderItem.Create(order.Id, Guid.NewGuid(), "Burger", unitPrice: 150_000m, discountPrice: null, quantity: 1));
        order.ClearDomainEvents();
        _dbContext.Orders.Add(order);
        await _dbContext.SaveChangesAsync();
        _dbContext.ChangeTracker.Clear();

        await _handler.Handle(raisedEvent, CancellationToken.None);

        await _paymentNotifier.Received(1).NotifyOrderPlacedAsync(
            order.Id,
            order.SlugId,
            Arg.Any<DateTime>(),
            order.TotalAmount,
            Arg.Any<int>(),
            Arg.Any<CancellationToken>());
        await _emailService.Received(1).SendNewOrderEmailAsync(
            OpsRecipient,
            order.SlugId,
            Arg.Any<DateTime>(),
            order.TotalAmount);
    }

    private static OrderCreatedEvent BuildEvent(DomainOrder order)
        => new(order.SlugId, order.UserId, order.TotalAmount, new List<Guid>());

    private async Task<(DomainOrder Order, Guid UserId)> SeedOrderAsync()
    {
        var user = Domain.User.User.Create("orderplaced@example.com", "hashedpassword", UserRole.User, "Jane", "Doe");
        user.ClearDomainEvents();
        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();

        var order = DomainOrder.Create(user.Id, Domain.Order.Enums.PaymentMethod.Banking, new List<Guid>());
        order.AddItem(Domain.Order.OrderItem.Create(order.Id, Guid.NewGuid(), "Burger", unitPrice: 150_000m, discountPrice: null, quantity: 1));
        order.ClearDomainEvents();
        _dbContext.Orders.Add(order);
        await _dbContext.SaveChangesAsync();
        _dbContext.ChangeTracker.Clear();

        return (order, user.Id);
    }
}