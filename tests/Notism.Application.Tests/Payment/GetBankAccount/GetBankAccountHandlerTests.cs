using FluentAssertions;

using Microsoft.Extensions.Logging;

using Notism.Application.Payment.GetBankAccount;
using Notism.Domain.Common.Specifications;
using Notism.Domain.Payment;

using NSubstitute;

namespace Notism.Application.Tests.Payment.GetBankAccount;

public class GetBankAccountHandlerTests
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly ILogger<GetBankAccountHandler> _logger;
    private readonly GetBankAccountHandler _handler;

    public GetBankAccountHandlerTests()
    {
        _paymentRepository = Substitute.For<IPaymentRepository>();
        _logger = Substitute.For<ILogger<GetBankAccountHandler>>();
        _handler = new GetBankAccountHandler(_paymentRepository, _logger);
    }

    [Fact]
    public async Task Handle_WhenPaymentExists_ReturnsResponse()
    {
        var storerId = Guid.NewGuid();
        var payment = Domain.Payment.Payment.Create(storerId, "Vietcombank", "123456789", "Nguyen Van A");

        _paymentRepository
            .FindByExpressionAsync(Arg.Any<FilterSpecification<Domain.Payment.Payment>>())
            .Returns(payment);

        var result = await _handler.Handle(new GetBankAccountRequest { StorerId = storerId }, CancellationToken.None);

        result.Should().NotBeNull();
        result!.BankCode.Should().Be("Vietcombank");
        result.AccountNumber.Should().Be("123456789");
        result.AccountHolderName.Should().Be("Nguyen Van A");
    }

    [Fact]
    public async Task Handle_WhenNoPayment_ReturnsNull()
    {
        var storerId = Guid.NewGuid();

        _paymentRepository
            .FindByExpressionAsync(Arg.Any<FilterSpecification<Domain.Payment.Payment>>())
            .Returns((Domain.Payment.Payment?)null);

        var result = await _handler.Handle(new GetBankAccountRequest { StorerId = storerId }, CancellationToken.None);

        result.Should().BeNull();
    }
}
