using FluentAssertions;

using MediatR;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using Notism.Application.Common.Persistence;
using Notism.Application.Common.Services;
using Notism.Application.Order.EventHandlers;
using Notism.Application.Tests.Common;
using Notism.Domain.Common.Persistence;
using Notism.Domain.Order.Enums;
using Notism.Domain.User.Enums;
using Notism.Infrastructure.Persistence;
using Notism.Shared.Configuration;

using NSubstitute;

using DomainOrder = Notism.Domain.Order.Order;

namespace Notism.Application.Tests.Order.EventHandlers;

// Exercises the SaveChangesAndCommitAsync seam against a real Postgres transaction: a buffered
// domain event must dispatch (firing the email + SignalR handlers) only after the commit succeeds,
// across both the direct SaveChangesAsync path and the UnitOfWork path.
public sealed class DomainEventDispatchTransactionTests : IClassFixture<PostgresReadDbContextFixture>, IAsyncLifetime
{
    private const string OpsRecipient = "ops@example.com";

    private readonly ServiceProvider _provider;
    private readonly AppDbContext _dbContext;
    private readonly IUnitOfWork _unitOfWork;
    private readonly INotifier _notifier;
    private readonly IEmailService _emailService;

    public DomainEventDispatchTransactionTests(PostgresReadDbContextFixture fixture)
    {
        _notifier = Substitute.For<INotifier>();
        _emailService = Substitute.For<IEmailService>();

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(OrderPlacedHandler).Assembly));
        services.AddSingleton(_notifier);
        services.AddSingleton(_emailService);
        services.AddSingleton(Options.Create(new EmailSettings { OpsRecipient = OpsRecipient }));
        services.AddSingleton(sp => new AppDbContext(
            new DbContextOptionsBuilder<AppDbContext>().UseNpgsql(fixture.ConnectionString).Options,
            sp.GetRequiredService<IMediator>()));
        services.AddSingleton<IReadDbContext>(sp => sp.GetRequiredService<AppDbContext>());
        services.AddSingleton<IUnitOfWork, UnitOfWork>();

        _provider = services.BuildServiceProvider();
        _dbContext = _provider.GetRequiredService<AppDbContext>();
        _unitOfWork = _provider.GetRequiredService<IUnitOfWork>();
    }

    public async Task InitializeAsync()
    {
        await _dbContext.Orders.ExecuteDeleteAsync();
        await _dbContext.Users.ExecuteDeleteAsync();
        _dbContext.ChangeTracker.Clear();
    }

    public async Task DisposeAsync()
    {
        await _provider.DisposeAsync();
    }

    [Fact]
    public async Task DirectSaveChanges_OnSuccessfulCommit_DispatchesBufferedEventOnce()
    {
        var userId = await SeedUserAsync();
        var order = DomainOrder.Create(userId, PaymentMethod.Banking, new List<Guid>());
        _dbContext.Orders.Add(order);

        await _dbContext.SaveChangesAsync();

        await _notifier.Received(1).NotifyOrderPlacedAsync(
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

    [Fact]
    public async Task UnitOfWork_OnSuccessfulCommit_DispatchesBufferedEventOnce()
    {
        var userId = await SeedUserAsync();
        DomainOrder order = null!;

        await _unitOfWork.ExecuteInTransactionAsync(() =>
        {
            order = DomainOrder.Create(userId, PaymentMethod.Banking, new List<Guid>());
            _dbContext.Orders.Add(order);

            return Task.CompletedTask;
        });

        await _notifier.Received(1).NotifyOrderPlacedAsync(
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

    [Fact]
    public async Task UnitOfWork_WhenOperationFailsAfterBuffering_SendsNoEmailOrNotificationAndRollsBack()
    {
        var userId = await SeedUserAsync();

        var act = async () => await _unitOfWork.ExecuteInTransactionAsync(async () =>
        {
            var order = DomainOrder.Create(userId, PaymentMethod.Banking, new List<Guid>());
            _dbContext.Orders.Add(order);
            await _dbContext.SaveChangesAsync();

            throw new InvalidOperationException("boom");
        });

        await act.Should().ThrowAsync<InvalidOperationException>();

        await _notifier.DidNotReceive().NotifyOrderPlacedAsync(
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

        _dbContext.ChangeTracker.Clear();
        (await _dbContext.Orders.CountAsync()).Should().Be(0);
    }

    [Fact]
    public async Task DirectSaveChanges_WhenPersistFails_SendsNoEmailOrNotification()
    {
        // No owning user row exists for this id, so the commit fails on the foreign-key constraint.
        var order = DomainOrder.Create(Guid.NewGuid(), PaymentMethod.Banking, new List<Guid>());
        _dbContext.Orders.Add(order);

        var act = async () => await _dbContext.SaveChangesAsync();

        await act.Should().ThrowAsync<DbUpdateException>();

        await _notifier.DidNotReceive().NotifyOrderPlacedAsync(
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

    private async Task<Guid> SeedUserAsync()
    {
        var user = Domain.User.User.Create($"dispatch-{Guid.NewGuid():N}@example.com", "hashedpassword", UserRole.User);
        user.ClearDomainEvents();
        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();
        _dbContext.ChangeTracker.Clear();

        return user.Id;
    }
}