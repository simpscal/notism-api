using FluentAssertions;

using Microsoft.Extensions.Logging;

using Notism.Application.Tests.Common;
using Notism.Application.User.SaveBankAccount;
using Notism.Domain.User.Enums;
using Notism.Infrastructure.Repositories;

using NSubstitute;

using DomainBankAccount = Notism.Domain.User.BankAccount;

namespace Notism.Application.Tests.User.SaveBankAccount;

public class SaveBankAccountHandlerTests : IDisposable
{
    private readonly WriteHandlerContext _context;
    private readonly SaveBankAccountHandler _handler;

    public SaveBankAccountHandlerTests()
    {
        _context = new WriteHandlerContext();
        _handler = new SaveBankAccountHandler(
            new BankAccountRepository(_context.DbContext),
            _context.DbContext,
            Substitute.For<ILogger<SaveBankAccountHandler>>());
    }

    [Fact]
    public async Task Handle_WhenNoExistingStoreBankAccount_CreatesStoreRow()
    {
        var ownerId = Guid.NewGuid();
        var request = new SaveBankAccountRequest
        {
            OwnerId = ownerId,
            OwnerType = BankAccountOwnerType.Store,
            BankCode = "Vietcombank",
            AccountNumber = "123456789",
            AccountHolderName = "Nguyen Van A",
        };

        await _handler.Handle(request, CancellationToken.None);

        _context.DbContext.ChangeTracker.Clear();
        var persisted = _context.DbContext.BankAccounts.Single(p => p.OwnerId == ownerId);
        persisted.OwnerType.Should().Be(BankAccountOwnerType.Store);
        persisted.BankCode.Should().Be("Vietcombank");
        persisted.AccountNumber.Should().Be("123456789");
        persisted.AccountHolderName.Should().Be("Nguyen Van A");
    }

    [Fact]
    public async Task Handle_WhenCustomerUpsertsOwnRow_CreatesCustomerRow()
    {
        var ownerId = Guid.NewGuid();
        var request = new SaveBankAccountRequest
        {
            OwnerId = ownerId,
            OwnerType = BankAccountOwnerType.Customer,
            BankCode = "Techcombank",
            AccountNumber = "987654321",
            AccountHolderName = "Tran Thi B",
        };

        await _handler.Handle(request, CancellationToken.None);

        _context.DbContext.ChangeTracker.Clear();
        var persisted = _context.DbContext.BankAccounts.Single(p => p.OwnerId == ownerId);
        persisted.OwnerType.Should().Be(BankAccountOwnerType.Customer);
        persisted.BankCode.Should().Be("Techcombank");
    }

    [Fact]
    public async Task Handle_WhenExistingBankAccountForOwner_UpdatesInPlace()
    {
        var ownerId = Guid.NewGuid();
        var existing = DomainBankAccount.Create(BankAccountOwnerType.Store, ownerId, "OldBank", "000", "Old Name");
        await _context.SeedAsync(existing);

        var request = new SaveBankAccountRequest
        {
            OwnerId = ownerId,
            OwnerType = BankAccountOwnerType.Store,
            BankCode = "Techcombank",
            AccountNumber = "987654321",
            AccountHolderName = "Tran Thi B",
        };

        await _handler.Handle(request, CancellationToken.None);

        _context.DbContext.ChangeTracker.Clear();
        var persisted = _context.DbContext.BankAccounts.Single(p => p.OwnerId == ownerId);
        persisted.BankCode.Should().Be("Techcombank");
        persisted.AccountNumber.Should().Be("987654321");
        persisted.AccountHolderName.Should().Be("Tran Thi B");
        _context.DbContext.BankAccounts.Should().ContainSingle(p => p.OwnerId == ownerId);
    }

    public void Dispose()
        => _context.Dispose();
}