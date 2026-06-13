using FluentAssertions;

using Microsoft.EntityFrameworkCore;

using Notism.Application.Tests.Common;
using Notism.Domain.Order.Enums;
using Notism.Domain.Payment.Enums;
using Notism.Domain.User.Enums;
using Notism.Infrastructure.Persistence;

using DomainOrder = Notism.Domain.Order.Order;

namespace Notism.Application.Tests.Payment.PaymentStatusPersistence;

public sealed class PaymentStatusPersistenceTests : IClassFixture<PostgresReadDbContextFixture>, IAsyncLifetime
{
    private readonly AppDbContext _dbContext;

    public PaymentStatusPersistenceTests(PostgresReadDbContextFixture fixture)
    {
        _dbContext = fixture.DbContext;
    }

    // The container is shared across the class; clear seeded orders before each test so
    // rows from one test never leak into another.
    public async Task InitializeAsync()
    {
        await _dbContext.Orders.ExecuteDeleteAsync();
        _dbContext.ChangeTracker.Clear();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task Refunded_RoundTripsThroughHasConversion_ReloadsAsRefunded()
    {
        var userId = await SeedUserAsync();
        var orderId = await SeedOrderAsync(userId, PaymentStatus.Refunded);

        _dbContext.ChangeTracker.Clear();
        var reloaded = await _dbContext.Orders.SingleAsync(o => o.Id == orderId);

        reloaded.PaymentStatus.Should().Be(PaymentStatus.Refunded);
    }

    [Fact]
    public async Task Refunded_PersistsAsRefundedStringInColumn()
    {
        var userId = await SeedUserAsync();
        var orderId = await SeedOrderAsync(userId, PaymentStatus.Refunded);

        _dbContext.ChangeTracker.Clear();
        var stored = await _dbContext.Database
            .SqlQuery<string>($"SELECT \"PaymentStatus\" AS \"Value\" FROM \"Orders\" WHERE \"Id\" = {orderId}")
            .SingleAsync();

        stored.Should().Be("refunded");
    }

    private async Task<Guid> SeedUserAsync()
    {
        var user = Domain.User.User.Create($"refund-{Guid.NewGuid():N}@example.com", "hashedpassword", UserRole.User);
        user.ClearDomainEvents();
        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();
        _dbContext.ChangeTracker.Clear();
        return user.Id;
    }

    private async Task<Guid> SeedOrderAsync(Guid userId, PaymentStatus paymentStatus)
    {
        var order = DomainOrder.Create(userId, PaymentMethod.Banking, new List<Guid>());
        order.ClearDomainEvents();
        typeof(DomainOrder).GetProperty(nameof(DomainOrder.PaymentStatus))!.SetValue(order, paymentStatus);
        _dbContext.Orders.Add(order);
        await _dbContext.SaveChangesAsync();
        _dbContext.ChangeTracker.Clear();
        return order.Id;
    }
}