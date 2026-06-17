using FluentAssertions;

using Microsoft.Extensions.Logging;

using Notism.Application.Payment.SaveBankAccount;
using Notism.Application.Tests.Common;
using Notism.Domain.Payment.Enums;
using Notism.Infrastructure.Repositories;

using NSubstitute;

using DomainPayment = Notism.Domain.Payment.Payment;

namespace Notism.Application.Tests.Payment.SaveBankAccount;

public class SaveBankAccountHandlerTests : IDisposable
{
    private readonly WriteHandlerContext _context;
    private readonly SaveBankAccountHandler _handler;

    public SaveBankAccountHandlerTests()
    {
        _context = new WriteHandlerContext();
        _handler = new SaveBankAccountHandler(
            new PaymentRepository(_context.DbContext),
            _context.DbContext,
            Substitute.For<ILogger<SaveBankAccountHandler>>());
    }

    [Fact]
    public async Task Handle_WhenNoExistingStorePayment_CreatesStoreRow()
    {
        var ownerId = Guid.NewGuid();
        var request = new SaveBankAccountRequest
        {
            OwnerId = ownerId,
            OwnerType = PaymentOwnerType.Store,
            BankCode = "Vietcombank",
            AccountNumber = "123456789",
            AccountHolderName = "Nguyen Van A",
        };

        await _handler.Handle(request, CancellationToken.None);

        _context.DbContext.ChangeTracker.Clear();
        var persisted = _context.DbContext.Payments.Single(p => p.StorerId == ownerId);
        persisted.OwnerType.Should().Be(PaymentOwnerType.Store);
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
            OwnerType = PaymentOwnerType.Customer,
            BankCode = "Techcombank",
            AccountNumber = "987654321",
            AccountHolderName = "Tran Thi B",
        };

        await _handler.Handle(request, CancellationToken.None);

        _context.DbContext.ChangeTracker.Clear();
        var persisted = _context.DbContext.Payments.Single(p => p.StorerId == ownerId);
        persisted.OwnerType.Should().Be(PaymentOwnerType.Customer);
        persisted.BankCode.Should().Be("Techcombank");
    }

    [Fact]
    public async Task Handle_WhenExistingPaymentForOwner_UpdatesInPlace()
    {
        var ownerId = Guid.NewGuid();
        var existing = DomainPayment.Create(PaymentOwnerType.Store, ownerId, "OldBank", "000", "Old Name");
        await _context.SeedAsync(existing);

        var request = new SaveBankAccountRequest
        {
            OwnerId = ownerId,
            OwnerType = PaymentOwnerType.Store,
            BankCode = "Techcombank",
            AccountNumber = "987654321",
            AccountHolderName = "Tran Thi B",
        };

        await _handler.Handle(request, CancellationToken.None);

        _context.DbContext.ChangeTracker.Clear();
        var persisted = _context.DbContext.Payments.Single(p => p.StorerId == ownerId);
        persisted.BankCode.Should().Be("Techcombank");
        persisted.AccountNumber.Should().Be("987654321");
        persisted.AccountHolderName.Should().Be("Tran Thi B");
        _context.DbContext.Payments.Should().ContainSingle(p => p.StorerId == ownerId);
    }

    public void Dispose()
        => _context.Dispose();
}