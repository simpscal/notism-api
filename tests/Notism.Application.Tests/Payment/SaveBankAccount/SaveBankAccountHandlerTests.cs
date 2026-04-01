using FluentAssertions;

using Microsoft.Extensions.Logging;

using Notism.Application.Payment.SaveBankAccount;
using Notism.Domain.Common.Specifications;
using Notism.Domain.Payment;

using NSubstitute;

namespace Notism.Application.Tests.Payment.SaveBankAccount;

public class SaveBankAccountHandlerTests
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly ILogger<SaveBankAccountHandler> _logger;
    private readonly SaveBankAccountHandler _handler;

    public SaveBankAccountHandlerTests()
    {
        _paymentRepository = Substitute.For<IPaymentRepository>();
        _logger = Substitute.For<ILogger<SaveBankAccountHandler>>();
        _handler = new SaveBankAccountHandler(_paymentRepository, _logger);
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

        _paymentRepository
            .FindByExpressionAsync(Arg.Any<FilterSpecification<Domain.Payment.Payment>>())
            .Returns((Domain.Payment.Payment?)null);

        await _handler.Handle(request, CancellationToken.None);

        _paymentRepository.Received(1).Add(Arg.Is<Domain.Payment.Payment>(p =>
            p.StorerId == storerId &&
            p.BankCode == "Vietcombank" &&
            p.AccountNumber == "123456789" &&
            p.AccountHolderName == "Nguyen Van A"));

        await _paymentRepository.Received(1).SaveChangesAsync();
    }

    [Fact]
    public async Task Handle_WhenExistingPayment_UpdatesPayment()
    {
        var storerId = Guid.NewGuid();
        var existing = Domain.Payment.Payment.Create(storerId, "OldBank", "000", "Old Name");
        var request = new SaveBankAccountRequest
        {
            StorerId = storerId,
            BankCode = "Techcombank",
            AccountNumber = "987654321",
            AccountHolderName = "Tran Thi B",
        };

        _paymentRepository
            .FindByExpressionAsync(Arg.Any<FilterSpecification<Domain.Payment.Payment>>())
            .Returns(existing);

        await _handler.Handle(request, CancellationToken.None);

        existing.BankCode.Should().Be("Techcombank");
        existing.AccountNumber.Should().Be("987654321");
        existing.AccountHolderName.Should().Be("Tran Thi B");

        _paymentRepository.DidNotReceive().Add(Arg.Any<Domain.Payment.Payment>());
        await _paymentRepository.Received(1).SaveChangesAsync();
    }
}
