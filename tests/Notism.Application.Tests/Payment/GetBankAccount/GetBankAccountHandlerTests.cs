using FluentAssertions;

using Notism.Application.Payment.GetBankAccount;
using Notism.Application.Tests.Common;
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
    public async Task Handle_WhenPaymentExists_ReturnsResponse()
    {
        var payment = DomainPayment.Create(Guid.NewGuid(), "Vietcombank", "123456789", "Nguyen Van A");
        payment.ClearDomainEvents();
        _dbContext.Payments.Add(payment);
        await _dbContext.SaveChangesAsync();
        _dbContext.ChangeTracker.Clear();

        var result = await _handler.Handle(new GetBankAccountRequest(), CancellationToken.None);

        result.Should().NotBeNull();
        result!.BankCode.Should().Be("Vietcombank");
        result.AccountNumber.Should().Be("123456789");
        result.AccountHolderName.Should().Be("Nguyen Van A");
    }

    [Fact]
    public async Task Handle_WhenNoPayment_ReturnsNull()
    {
        var result = await _handler.Handle(new GetBankAccountRequest(), CancellationToken.None);

        result.Should().BeNull();
    }
}