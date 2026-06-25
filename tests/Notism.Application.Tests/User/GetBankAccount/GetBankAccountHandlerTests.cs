using FluentAssertions;

using Notism.Application.Tests.Common;
using Notism.Application.User.GetBankAccount;
using Notism.Domain.User.Enums;
using Notism.Infrastructure.Persistence;

using DomainBankAccount = Notism.Domain.User.BankAccount;

namespace Notism.Application.Tests.User.GetBankAccount;

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
        await SeedBankAccountAsync(DomainBankAccount.Create(BankAccountOwnerType.Store, storeOwnerId, "Vietcombank", "123456789", "Nguyen Van A"));

        var result = await _handler.Handle(
            new GetBankAccountRequest { OwnerId = storeOwnerId, OwnerType = BankAccountOwnerType.Store },
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
        await SeedBankAccountAsync(DomainBankAccount.Create(BankAccountOwnerType.Store, Guid.NewGuid(), "Vietcombank", "111", "Store"));
        await SeedBankAccountAsync(DomainBankAccount.Create(BankAccountOwnerType.Customer, customerId, "Techcombank", "222", "Tran Thi B"));

        var result = await _handler.Handle(
            new GetBankAccountRequest { OwnerId = customerId, OwnerType = BankAccountOwnerType.Customer },
            CancellationToken.None);

        result.Should().NotBeNull();
        result!.BankCode.Should().Be("Techcombank");
        result.AccountNumber.Should().Be("222");
    }

    [Fact]
    public async Task Handle_WhenNoRowForCaller_ReturnsNull()
    {
        await SeedBankAccountAsync(DomainBankAccount.Create(BankAccountOwnerType.Customer, Guid.NewGuid(), "Techcombank", "222", "Other"));

        var result = await _handler.Handle(
            new GetBankAccountRequest { OwnerId = Guid.NewGuid(), OwnerType = BankAccountOwnerType.Customer },
            CancellationToken.None);

        result.Should().BeNull();
    }

    private async Task SeedBankAccountAsync(DomainBankAccount bankAccount)
    {
        bankAccount.ClearDomainEvents();
        _dbContext.BankAccounts.Add(bankAccount);
        await _dbContext.SaveChangesAsync();
        _dbContext.ChangeTracker.Clear();
    }
}