using FluentAssertions;

using Microsoft.Extensions.Logging;

using Notism.Application.Payment.GetBankAccount;
using Notism.Domain.Common.Specifications;
using Notism.Domain.Payment;
using Notism.Shared.Exceptions;

using NSubstitute;

namespace Notism.Application.Tests.Payment.GetBankAccount;

public class GetBankAccountHandlerTests
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly IBankingCheckoutRepository _bankingCheckoutRepository;
    private readonly ILogger<GetBankAccountHandler> _logger;
    private readonly GetBankAccountHandler _handler;

    public GetBankAccountHandlerTests()
    {
        _paymentRepository = Substitute.For<IPaymentRepository>();
        _bankingCheckoutRepository = Substitute.For<IBankingCheckoutRepository>();
        _logger = Substitute.For<ILogger<GetBankAccountHandler>>();
        _handler = new GetBankAccountHandler(_paymentRepository, _bankingCheckoutRepository, _logger);
    }

    // --- Admin path ---
    [Fact]
    public async Task Handle_WhenAdminAndPaymentExists_ReturnsResponse()
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
    public async Task Handle_WhenAdminAndNoPayment_ReturnsNull()
    {
        var storerId = Guid.NewGuid();

        _paymentRepository
            .FindByExpressionAsync(Arg.Any<FilterSpecification<Domain.Payment.Payment>>())
            .Returns((Domain.Payment.Payment?)null);

        var result = await _handler.Handle(new GetBankAccountRequest { StorerId = storerId }, CancellationToken.None);

        result.Should().BeNull();
    }

    // --- Consumer path ---
    [Fact]
    public async Task Handle_WhenConsumerWithValidCheckout_ReturnsBankAccount()
    {
        var consumerId = Guid.NewGuid();
        var checkoutId = Guid.NewGuid();
        var storerId = Guid.NewGuid();

        var checkout = BankingCheckout.Create(consumerId, new List<Guid> { Guid.NewGuid() }, 100_000m);
        var payment = Domain.Payment.Payment.Create(storerId, "Techcombank", "987654321", "Tran Thi B");

        _bankingCheckoutRepository
            .FindByExpressionAsync(Arg.Any<FilterSpecification<BankingCheckout>>())
            .Returns(checkout);

        _paymentRepository
            .FindByExpressionAsync(Arg.Any<FilterSpecification<Domain.Payment.Payment>>())
            .Returns(payment);

        var result = await _handler.Handle(
            new GetBankAccountRequest { CheckoutId = checkoutId, UserId = consumerId },
            CancellationToken.None);

        result.Should().NotBeNull();
        result!.BankCode.Should().Be("Techcombank");
        result.AccountNumber.Should().Be("987654321");
        result.AccountHolderName.Should().Be("Tran Thi B");
    }

    [Fact]
    public async Task Handle_WhenConsumerCheckoutNotFound_ThrowsNotFoundException()
    {
        var consumerId = Guid.NewGuid();
        var checkoutId = Guid.NewGuid();

        _bankingCheckoutRepository
            .FindByExpressionAsync(Arg.Any<FilterSpecification<BankingCheckout>>())
            .Returns((BankingCheckout?)null);

        var act = async () => await _handler.Handle(
            new GetBankAccountRequest { CheckoutId = checkoutId, UserId = consumerId },
            CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenConsumerCheckoutBelongsToDifferentUser_ThrowsForbiddenException()
    {
        var consumerId = Guid.NewGuid();
        var differentUserId = Guid.NewGuid();
        var checkoutId = Guid.NewGuid();

        // Checkout was created by a different user.
        var checkout = BankingCheckout.Create(differentUserId, new List<Guid> { Guid.NewGuid() }, 50_000m);

        _bankingCheckoutRepository
            .FindByExpressionAsync(Arg.Any<FilterSpecification<BankingCheckout>>())
            .Returns(checkout);

        var act = async () => await _handler.Handle(
            new GetBankAccountRequest { CheckoutId = checkoutId, UserId = consumerId },
            CancellationToken.None);

        await act.Should().ThrowAsync<ForbiddenException>();
    }
}
