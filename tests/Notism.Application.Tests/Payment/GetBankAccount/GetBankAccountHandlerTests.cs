using FluentAssertions;

using Notism.Application.Payment.GetBankAccount;
using Notism.Application.Tests.Common;
using Notism.Domain.Payment.Enums;
using Notism.Infrastructure.Persistence;

using DomainPayment = Notism.Domain.Payment.Payment;

namespace Notism.Application.Tests.Payment.GetBankAccount;

public class GetBankAccountHandlerTests
{
    private readonly AppDbContext _dbContext;
    private readonly GetBankAccountHandler _handler;

    public GetBankAccountHandlerTests()
    {
        _dbContext = ReadDbContextFactory.Create();
        _handler = new GetBankAccountHandler(_dbContext);
    }

    [Fact]
    public async Task Handle_WhenStoreRowExists_ReturnsStoreRowForAdmin()
    {
        var storeOwnerId = Guid.NewGuid();
        await SeedPaymentAsync(DomainPayment.Create(PaymentOwnerType.Store, storeOwnerId, "Vietcombank", "123456789", "Nguyen Van A"));

        var result = await _handler.Handle(
            new GetBankAccountRequest { OwnerId = storeOwnerId, OwnerType = PaymentOwnerType.Store },
            CancellationToken.None);

        result.Should().NotBeNull();
        result!.BankCode.Should().Be("Vietcombank");
        result.AccountNumber.Should().Be("123456789");
        result.AccountHolderName.Should().Be("Nguyen Van A");
    }

    [Fact]
    public async Task Handle_WhenCustomerRowExists_ReturnsOnlyOwnRow()
    {
        var customerId = Guid.NewGuid();
        await SeedPaymentAsync(DomainPayment.Create(PaymentOwnerType.Store, Guid.NewGuid(), "Vietcombank", "111", "Store"));
        await SeedPaymentAsync(DomainPayment.Create(PaymentOwnerType.Customer, customerId, "Techcombank", "222", "Tran Thi B"));

        var result = await _handler.Handle(
            new GetBankAccountRequest { OwnerId = customerId, OwnerType = PaymentOwnerType.Customer },
            CancellationToken.None);

        result.Should().NotBeNull();
        result!.BankCode.Should().Be("Techcombank");
        result.AccountNumber.Should().Be("222");
    }

    [Fact]
    public async Task Handle_WhenNoRowForCaller_ReturnsNull()
    {
        await SeedPaymentAsync(DomainPayment.Create(PaymentOwnerType.Customer, Guid.NewGuid(), "Techcombank", "222", "Other"));

        var result = await _handler.Handle(
            new GetBankAccountRequest { OwnerId = Guid.NewGuid(), OwnerType = PaymentOwnerType.Customer },
            CancellationToken.None);

        result.Should().BeNull();
    }

    private async Task SeedPaymentAsync(DomainPayment payment)
    {
        payment.ClearDomainEvents();
        _dbContext.Payments.Add(payment);
        await _dbContext.SaveChangesAsync();
        _dbContext.ChangeTracker.Clear();
    }
}