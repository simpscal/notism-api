using FluentAssertions;

using Microsoft.Extensions.Logging;

using Notism.Application.Payment.SaveBankAccount;
using Notism.Application.Tests.Common;
using Notism.Infrastructure.Repositories;

using NSubstitute;

namespace Notism.Application.Tests.Payment.SaveBankAccount;

/// <summary>
/// Exercises the handler over an EF InMemory database: an existing account is loaded
/// TRACKED through the read port and updated in place, or a new one is added — either way
/// persisted via the repository SaveChanges on the same context.
/// </summary>
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
    public async Task Handle_WhenNoExistingPayment_CreatesNewPayment()
    {
        var storerId = Guid.NewGuid();
        var request = new SaveBankAccountRequest
        {
            StorerId = storerId,
            BankCode = "Vietcombank",
            AccountNumber = "123456789",
            AccountHolderName = "Nguyen Van A",
        };

        await _handler.Handle(request, CancellationToken.None);

        _context.DbContext.ChangeTracker.Clear();
        var persisted = _context.DbContext.Payments.Single(p => p.StorerId == storerId);
        persisted.BankCode.Should().Be("Vietcombank");
        persisted.AccountNumber.Should().Be("123456789");
        persisted.AccountHolderName.Should().Be("Nguyen Van A");
    }

    [Fact]
    public async Task Handle_WhenExistingPayment_UpdatesPayment()
    {
        var storerId = Guid.NewGuid();
        var existing = Domain.Payment.Payment.Create(storerId, "OldBank", "000", "Old Name");
        await _context.SeedAsync(existing);

        var request = new SaveBankAccountRequest
        {
            StorerId = storerId,
            BankCode = "Techcombank",
            AccountNumber = "987654321",
            AccountHolderName = "Tran Thi B",
        };

        await _handler.Handle(request, CancellationToken.None);

        _context.DbContext.ChangeTracker.Clear();
        var persisted = _context.DbContext.Payments.Single(p => p.StorerId == storerId);
        persisted.BankCode.Should().Be("Techcombank");
        persisted.AccountNumber.Should().Be("987654321");
        persisted.AccountHolderName.Should().Be("Tran Thi B");
        _context.DbContext.Payments.Should().ContainSingle(p => p.StorerId == storerId);
    }

    public void Dispose()
        => _context.Dispose();
}
