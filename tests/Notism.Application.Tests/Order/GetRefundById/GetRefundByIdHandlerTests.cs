using FluentAssertions;

using Microsoft.Extensions.Logging;

using Notism.Application.Common.Services;
using Notism.Application.Order.GetRefundById;
using Notism.Application.Tests.Common;
using Notism.Infrastructure.Persistence;
using Notism.Shared.Exceptions;

using NSubstitute;

using BankAccountOwnerType = Notism.Domain.User.Enums.BankAccountOwnerType;
using DomainBankAccount = Notism.Domain.User.BankAccount;
using DomainOrder = Notism.Domain.Order.Order;
using DomainUser = Notism.Domain.User.User;
using PaymentMethodEnum = Notism.Domain.Order.Enums.PaymentMethod;
using UserRoleEnum = Notism.Domain.User.Enums.UserRole;

namespace Notism.Application.Tests.Order.GetRefundById;

public class GetRefundByIdHandlerTests : IDisposable
{
    private readonly AppDbContext _dbContext;
    private readonly IMessages _messages;
    private readonly GetRefundByIdHandler _handler;

    public GetRefundByIdHandlerTests()
    {
        _dbContext = ReadDbContextFactory.Create();
        _messages = Substitute.For<IMessages>();
        _messages.RefundNotFound.Returns("Refund not found.");

        _handler = new GetRefundByIdHandler(
            _dbContext,
            Substitute.For<ILogger<GetRefundByIdHandler>>(),
            _messages);
    }

    [Fact]
    public async Task Handle_WhenRefundProcessing_ReturnsDetailWithAmountToTransferAndCustomer()
    {
        var (order, _) = await SeedOrderWithRefundAsync(processToPaid: false);

        var result = await _handler.Handle(
            new GetRefundByIdRequest { RefundId = order.Refund!.Id },
            CancellationToken.None);

        result.Id.Should().Be(order.Refund.Id);
        result.OrderId.Should().Be(order.Id);
        result.OrderSlugId.Should().Be(order.SlugId);
        result.Amount.Should().Be(150_000m);
        result.Status.Should().Be("processing");
        result.CustomerEmail.Should().Be("buyer@example.com");
        result.TransferReference.Should().BeNull();
        result.PaidAt.Should().BeNull();
    }

    [Fact]
    public async Task Handle_WhenRefundPaid_SurfacesTransferReferenceAndPaidAt()
    {
        var (order, _) = await SeedOrderWithRefundAsync(processToPaid: true);

        var result = await _handler.Handle(
            new GetRefundByIdRequest { RefundId = order.Refund!.Id },
            CancellationToken.None);

        result.Status.Should().Be("paid");
        result.TransferReference.Should().Be("SEPAY-REF-1");
        result.PaidAt.Should().NotBeNull();
    }

    [Fact]
    public async Task Handle_WhenCustomerHasPayoutRow_ReturnsPayoutFields()
    {
        var (order, userId) = await SeedOrderWithRefundAsync(processToPaid: false);
        await SeedCustomerPayoutAsync(userId, "Techcombank", "987654321", "Jane Doe");

        var result = await _handler.Handle(
            new GetRefundByIdRequest { RefundId = order.Refund!.Id },
            CancellationToken.None);

        result.BankCode.Should().Be("Techcombank");
        result.AccountNumber.Should().Be("987654321");
        result.AccountHolderName.Should().Be("Jane Doe");
    }

    [Fact]
    public async Task Handle_WhenNoCustomerPayoutRow_ReturnsNullPayoutFields()
    {
        var (order, _) = await SeedOrderWithRefundAsync(processToPaid: false);

        // A Store row exists but must not be resolved as the customer's payout.
        await SeedPaymentAsync(DomainBankAccount.Create(BankAccountOwnerType.Store, Guid.NewGuid(), "Vietcombank", "111", "Store"));

        var result = await _handler.Handle(
            new GetRefundByIdRequest { RefundId = order.Refund!.Id },
            CancellationToken.None);

        result.BankCode.Should().BeNull();
        result.AccountNumber.Should().BeNull();
        result.AccountHolderName.Should().BeNull();
    }

    [Fact]
    public async Task Handle_WhenRefundNotFound_ThrowsNotFound()
    {
        var act = async () => await _handler.Handle(
            new GetRefundByIdRequest { RefundId = Guid.NewGuid() },
            CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    public void Dispose()
        => _dbContext.Dispose();

    private async Task<(DomainOrder Order, Guid UserId)> SeedOrderWithRefundAsync(bool processToPaid)
    {
        var user = DomainUser.Create("buyer@example.com", "hashedpassword", UserRoleEnum.User, "Jane", "Doe");
        user.ClearDomainEvents();
        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();

        var order = DomainOrder.Create(user.Id, PaymentMethodEnum.Banking, new List<Guid>());
        order.AddItem(Domain.Order.OrderItem.Create(order.Id, Guid.NewGuid(), "Burger", unitPrice: 150_000m, discountPrice: null, quantity: 1));
        order.MarkAsPaid(DateTime.UtcNow);
        order.CreateRefund();
        order.MarkRefundProcessing();

        if (processToPaid)
        {
            order.MarkRefundPaid("SEPAY-REF-1");
        }

        order.ClearDomainEvents();
        _dbContext.Orders.Add(order);
        await _dbContext.SaveChangesAsync();
        _dbContext.ChangeTracker.Clear();

        return (order, user.Id);
    }

    private async Task SeedCustomerPayoutAsync(Guid ownerId, string bankCode, string accountNumber, string accountHolderName)
        => await SeedPaymentAsync(DomainBankAccount.Create(BankAccountOwnerType.Customer, ownerId, bankCode, accountNumber, accountHolderName));

    private async Task SeedPaymentAsync(DomainBankAccount payment)
    {
        payment.ClearDomainEvents();
        _dbContext.BankAccounts.Add(payment);
        await _dbContext.SaveChangesAsync();
        _dbContext.ChangeTracker.Clear();
    }
}