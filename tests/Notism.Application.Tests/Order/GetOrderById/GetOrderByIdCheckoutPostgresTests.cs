using FluentAssertions;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using Notism.Application.Common.Services;
using Notism.Application.Order.GetOrderById;
using Notism.Application.Tests.Common;
using Notism.Domain.Order.Enums;
using Notism.Domain.Payment.Enums;
using Notism.Domain.User.Enums;
using Notism.Infrastructure.Persistence;

using NSubstitute;

using DomainOrder = Notism.Domain.Order.Order;
using DomainPayment = Notism.Domain.Payment.Payment;
using DomainUser = Notism.Domain.User.User;

namespace Notism.Application.Tests.Order.GetOrderById;

// Real-Postgres coverage: SQLite/InMemory providers do not exercise the Npgsql
// translation of the OwnerType filter that selects the Store row at checkout.
public sealed class GetOrderByIdCheckoutPostgresTests : IClassFixture<PostgresReadDbContextFixture>, IAsyncLifetime
{
    private readonly AppDbContext _dbContext;
    private readonly GetOrderByIdHandler _handler;

    public GetOrderByIdCheckoutPostgresTests(PostgresReadDbContextFixture fixture)
    {
        _dbContext = fixture.DbContext;

        var messages = Substitute.For<IMessages>();
        messages.OrderNotFound.Returns("Order not found.");

        _handler = new GetOrderByIdHandler(
            _dbContext,
            Substitute.For<IStorageService>(),
            Substitute.For<ILogger<GetOrderByIdHandler>>(),
            messages);
    }

    public async Task InitializeAsync()
    {
        await _dbContext.Orders.ExecuteDeleteAsync();
        await _dbContext.Payments.ExecuteDeleteAsync();
        _dbContext.ChangeTracker.Clear();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task Handle_WithCustomerRowPresent_ResolvesStoreRowForCheckoutQr()
    {
        var userId = await SeedUserAsync();
        var order = DomainOrder.Create(userId, PaymentMethod.Banking, new List<Guid>());
        order.ClearDomainEvents();
        _dbContext.Orders.Add(order);

        _dbContext.Payments.Add(SeedReady(DomainPayment.Create(PaymentOwnerType.Customer, userId, "CustomerBank", "000", "Customer")));
        _dbContext.Payments.Add(SeedReady(DomainPayment.Create(PaymentOwnerType.Store, Guid.NewGuid(), "Vietcombank", "123456789", "Store Account")));
        await _dbContext.SaveChangesAsync();
        _dbContext.ChangeTracker.Clear();

        var result = await _handler.Handle(
            new GetOrderByIdRequest { SlugId = order.SlugId, UserId = userId, Role = "user" },
            CancellationToken.None);

        result.PaymentQr.Should().NotBeNull();
        result.PaymentQr!.BankCode.Should().Be("Vietcombank");
        result.PaymentQr.AccountNumber.Should().Be("123456789");
        result.PaymentQr.AccountHolderName.Should().Be("Store Account");
    }

    private async Task<Guid> SeedUserAsync()
    {
        var user = DomainUser.Create($"checkout-{Guid.NewGuid():N}@example.com", "hashedpassword", UserRole.User);
        user.ClearDomainEvents();
        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();
        _dbContext.ChangeTracker.Clear();
        return user.Id;
    }

    private static DomainPayment SeedReady(DomainPayment payment)
    {
        payment.ClearDomainEvents();
        return payment;
    }
}