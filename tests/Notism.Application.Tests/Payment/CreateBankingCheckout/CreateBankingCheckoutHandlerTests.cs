using FluentAssertions;

using Notism.Application.Payment.CreateBankingCheckout;
using Notism.Domain.Common.Specifications;
using Notism.Domain.Payment;

using NSubstitute;

namespace Notism.Application.Tests.Payment.CreateBankingCheckout;

public class CreateBankingCheckoutHandlerTests
{
    private readonly IBankingCheckoutRepository _bankingCheckoutRepository;
    private readonly CreateBankingCheckoutHandler _handler;

    public CreateBankingCheckoutHandlerTests()
    {
        _bankingCheckoutRepository = Substitute.For<IBankingCheckoutRepository>();
        _handler = new CreateBankingCheckoutHandler(_bankingCheckoutRepository);
    }

    [Fact]
    public async Task Handle_WhenValidRequest_CreatesBankingCheckoutAndReturnsCheckoutId()
    {
        var userId = Guid.NewGuid();
        var cartItemIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };
        var totalAmount = 150_000m;

        BankingCheckout? addedCheckout = null;
        await _bankingCheckoutRepository
            .AddAsync(Arg.Do<BankingCheckout>(c => addedCheckout = c));

        var request = new CreateBankingCheckoutRequest
        {
            UserId = userId,
            CartItemIds = cartItemIds,
            TotalAmount = totalAmount,
        };

        var response = await _handler.Handle(request, CancellationToken.None);

        response.Should().NotBeNull();
        addedCheckout.Should().NotBeNull();
        addedCheckout!.UserId.Should().Be(userId);
        addedCheckout.CartItemIds.Should().BeEquivalentTo(cartItemIds);
        addedCheckout.TotalAmount.Should().Be(totalAmount);
        addedCheckout.IsUsed.Should().BeFalse();
        await _bankingCheckoutRepository.Received(1).SaveChangesAsync();
    }

    [Fact]
    public async Task Handle_WhenValidRequest_PersistsCheckoutWithIsUsedFalse()
    {
        var userId = Guid.NewGuid();
        var cartItemIds = new List<Guid> { Guid.NewGuid() };

        BankingCheckout? addedCheckout = null;
        await _bankingCheckoutRepository
            .AddAsync(Arg.Do<BankingCheckout>(c => addedCheckout = c));

        var request = new CreateBankingCheckoutRequest
        {
            UserId = userId,
            CartItemIds = cartItemIds,
            TotalAmount = 50_000m,
        };

        await _handler.Handle(request, CancellationToken.None);

        addedCheckout!.IsUsed.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_WhenValidRequest_ReturnedCheckoutIdMatchesPersistedId()
    {
        var userId = Guid.NewGuid();

        BankingCheckout? addedCheckout = null;
        await _bankingCheckoutRepository
            .AddAsync(Arg.Do<BankingCheckout>(c => addedCheckout = c));

        var request = new CreateBankingCheckoutRequest
        {
            UserId = userId,
            CartItemIds = new List<Guid> { Guid.NewGuid() },
            TotalAmount = 80_000m,
        };

        var response = await _handler.Handle(request, CancellationToken.None);

        response.CheckoutId.Should().Be(addedCheckout!.Id);
    }
}
