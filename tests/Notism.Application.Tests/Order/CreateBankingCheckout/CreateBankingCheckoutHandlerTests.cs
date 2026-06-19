using FluentAssertions;

using Notism.Application.Order.CreateBankingCheckout;
using Notism.Application.Tests.Common;
using Notism.Domain.Order;
using Notism.Domain.Order.Repositories;
using Notism.Domain.User.Enums;
using Notism.Infrastructure.Persistence;

using NSubstitute;

using DomainBankAccount = Notism.Domain.User.BankAccount;

namespace Notism.Application.Tests.Order.CreateBankingCheckout;

public class CreateBankingCheckoutHandlerTests
{
    private readonly IBankingCheckoutRepository _bankingCheckoutRepository;
    private readonly AppDbContext _dbContext;
    private readonly CreateBankingCheckoutHandler _handler;

    public CreateBankingCheckoutHandlerTests()
    {
        _bankingCheckoutRepository = Substitute.For<IBankingCheckoutRepository>();
        _dbContext = ReadDbContextFactory.Create();
        _handler = new CreateBankingCheckoutHandler(_bankingCheckoutRepository, _dbContext);
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

    [Fact]
    public async Task Handle_WhenStoreAccountConfigured_ReturnsStoreBankAccount()
    {
        await SeedBankAccountAsync(DomainBankAccount.Create(BankAccountOwnerType.Store, Guid.NewGuid(), "Vietcombank", "123456789", "Store Account"));

        var request = new CreateBankingCheckoutRequest
        {
            UserId = Guid.NewGuid(),
            CartItemIds = new List<Guid> { Guid.NewGuid() },
            TotalAmount = 120_000m,
        };

        var response = await _handler.Handle(request, CancellationToken.None);

        response.BankAccount.Should().NotBeNull();
        response.BankAccount!.BankCode.Should().Be("Vietcombank");
        response.BankAccount.AccountNumber.Should().Be("123456789");
        response.BankAccount.AccountHolderName.Should().Be("Store Account");
    }

    [Fact]
    public async Task Handle_WhenOnlyCustomerRowPresent_ReturnsNullBankAccount()
    {
        var customerId = Guid.NewGuid();
        await SeedBankAccountAsync(DomainBankAccount.Create(BankAccountOwnerType.Customer, customerId, "CustomerBank", "000", "Customer"));

        var request = new CreateBankingCheckoutRequest
        {
            UserId = customerId,
            CartItemIds = new List<Guid> { Guid.NewGuid() },
            TotalAmount = 90_000m,
        };

        var response = await _handler.Handle(request, CancellationToken.None);

        response.BankAccount.Should().BeNull();
    }

    [Fact]
    public async Task Handle_WhenNoStoreAccountConfigured_ReturnsNullBankAccount()
    {
        var request = new CreateBankingCheckoutRequest
        {
            UserId = Guid.NewGuid(),
            CartItemIds = new List<Guid> { Guid.NewGuid() },
            TotalAmount = 70_000m,
        };

        var response = await _handler.Handle(request, CancellationToken.None);

        response.BankAccount.Should().BeNull();
    }

    private async Task SeedBankAccountAsync(DomainBankAccount bankAccount)
    {
        bankAccount.ClearDomainEvents();
        _dbContext.BankAccounts.Add(bankAccount);
        await _dbContext.SaveChangesAsync();
        _dbContext.ChangeTracker.Clear();
    }
}
