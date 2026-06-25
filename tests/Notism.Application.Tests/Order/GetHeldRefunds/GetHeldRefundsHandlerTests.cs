using FluentAssertions;

using Microsoft.Extensions.Logging;

using Notism.Application.Order.GetHeldRefunds;
using Notism.Application.Tests.Common;
using Notism.Infrastructure.Persistence;

using NSubstitute;

using BankAccountOwnerType = Notism.Domain.User.Enums.BankAccountOwnerType;
using DomainBankAccount = Notism.Domain.User.BankAccount;
using DomainOrder = Notism.Domain.Order.Order;
using DomainUser = Notism.Domain.User.User;
using PaymentMethodEnum = Notism.Domain.Order.Enums.PaymentMethod;
using RefundState = Notism.Domain.Order.Enums.RefundStatus;
using UserRoleEnum = Notism.Domain.User.Enums.UserRole;

namespace Notism.Application.Tests.Order.GetHeldRefunds;

public class GetHeldRefundsHandlerTests : IDisposable
{
    private readonly AppDbContext _dbContext;
    private readonly GetHeldRefundsHandler _handler;

    public GetHeldRefundsHandlerTests()
    {
        _dbContext = ReadDbContextFactory.Create();

        _handler = new GetHeldRefundsHandler(
            _dbContext,
            Substitute.For<ILogger<GetHeldRefundsHandler>>());
    }

    [Fact]
    public async Task Handle_WhenCallerHasBankDetails_ReturnsEmptyEvenWithHeldRefunds()
    {
        var userId = await SeedUserAsync();
        await SeedOrderWithRefundAsync(userId, RefundState.Pending);
        await SeedCustomerPayoutAsync(userId);

        var result = await _handler.Handle(
            new GetHeldRefundsRequest { UserId = userId },
            CancellationToken.None);

        result.Items.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_WhenNoBankDetailsAndPendingRefund_ReturnsHeldRefund()
    {
        var userId = await SeedUserAsync();
        var order = await SeedOrderWithRefundAsync(userId, RefundState.Pending);

        var result = await _handler.Handle(
            new GetHeldRefundsRequest { UserId = userId },
            CancellationToken.None);

        result.Items.Should().ContainSingle();
        var item = result.Items.Single();
        item.RefundId.Should().Be(order.Refund!.Id);
        item.OrderReference.Should().Be(order.SlugId);
        item.Amount.Should().Be(150_000m);
    }

    [Fact]
    public async Task Handle_WhenNoBankDetailsAndProcessingRefund_ReturnsHeldRefund()
    {
        var userId = await SeedUserAsync();
        var order = await SeedOrderWithRefundAsync(userId, RefundState.Processing);

        var result = await _handler.Handle(
            new GetHeldRefundsRequest { UserId = userId },
            CancellationToken.None);

        result.Items.Should().ContainSingle();
        result.Items.Single().RefundId.Should().Be(order.Refund!.Id);
    }

    [Fact]
    public async Task Handle_WhenNoBankDetailsAndOnlyPaidOrFailedRefunds_ReturnsEmpty()
    {
        var userId = await SeedUserAsync();
        await SeedOrderWithRefundAsync(userId, RefundState.Paid);
        await SeedOrderWithRefundAsync(userId, RefundState.Failed);

        var result = await _handler.Handle(
            new GetHeldRefundsRequest { UserId = userId },
            CancellationToken.None);

        result.Items.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_WhenNoBankDetailsAndNoRefunds_ReturnsEmpty()
    {
        var userId = await SeedUserAsync();

        var result = await _handler.Handle(
            new GetHeldRefundsRequest { UserId = userId },
            CancellationToken.None);

        result.Items.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_WhenAnotherUserHasHeldRefund_ExcludesIt()
    {
        var userId = await SeedUserAsync();
        var otherUserId = await SeedUserAsync("other@example.com");
        await SeedOrderWithRefundAsync(otherUserId, RefundState.Pending);

        var result = await _handler.Handle(
            new GetHeldRefundsRequest { UserId = userId },
            CancellationToken.None);

        result.Items.Should().BeEmpty();
    }

    public void Dispose()
        => _dbContext.Dispose();

    private async Task<Guid> SeedUserAsync(string email = "buyer@example.com")
    {
        var user = DomainUser.Create(email, "hashedpassword", UserRoleEnum.User, "Jane", "Doe");
        user.ClearDomainEvents();
        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();
        _dbContext.ChangeTracker.Clear();

        return user.Id;
    }

    private async Task<DomainOrder> SeedOrderWithRefundAsync(Guid userId, RefundState status)
    {
        var order = DomainOrder.Create(userId, PaymentMethodEnum.Banking, new List<Guid>());
        order.AddItem(Domain.Order.OrderItem.Create(order.Id, Guid.NewGuid(), "Burger", unitPrice: 150_000m, discountPrice: null, quantity: 1));
        order.MarkAsPaid(DateTime.UtcNow);
        order.CreateRefund();

        switch (status)
        {
            case RefundState.Pending:
                break;
            case RefundState.Processing:
                order.MarkRefundProcessing();
                break;
            case RefundState.Paid:
                order.MarkRefundProcessing();
                order.MarkRefundPaid("SEPAY-REF-1");
                break;
            case RefundState.Failed:
                order.MarkRefundProcessing();
                order.MarkRefundFailed("bank rejected");
                break;
        }

        order.ClearDomainEvents();
        _dbContext.Orders.Add(order);
        await _dbContext.SaveChangesAsync();
        _dbContext.ChangeTracker.Clear();

        return order;
    }

    private async Task SeedCustomerPayoutAsync(Guid userId)
    {
        var payment = DomainBankAccount.Create(BankAccountOwnerType.Customer, userId, "Techcombank", "987654321", "Jane Doe");
        payment.ClearDomainEvents();
        _dbContext.BankAccounts.Add(payment);
        await _dbContext.SaveChangesAsync();
        _dbContext.ChangeTracker.Clear();
    }
}