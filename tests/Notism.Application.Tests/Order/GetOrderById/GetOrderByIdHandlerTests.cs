using FluentAssertions;

using Microsoft.Extensions.Logging;

using Notism.Application.Common.Services;
using Notism.Application.Order.GetOrderById;
using Notism.Application.Tests.Common;
using Notism.Domain.Order.Enums;
using Notism.Domain.Payment.Enums;
using Notism.Domain.User.Enums;
using Notism.Infrastructure.Persistence;
using Notism.Shared.Exceptions;

using NSubstitute;

using DomainOrder = Notism.Domain.Order.Order;
using DomainPayment = Notism.Domain.Payment.Payment;

namespace Notism.Application.Tests.Order.GetOrderById;

public class GetOrderByIdHandlerTests
{
    private readonly AppDbContext _dbContext;
    private readonly IMessages _messages;
    private readonly GetOrderByIdHandler _handler;
    private Guid _userId;

    public GetOrderByIdHandlerTests()
    {
        _dbContext = ReadDbContextFactory.Create();
        _messages = Substitute.For<IMessages>();
        _messages.OrderNotFound.Returns("Order not found.");

        _handler = new GetOrderByIdHandler(
            _dbContext,
            Substitute.For<IStorageService>(),
            Substitute.For<ILogger<GetOrderByIdHandler>>(),
            _messages);
    }

    [Fact]
    public async Task Handle_WhenOrderIsBankingAndUnpaidAndPaymentExists_ReturnsPaymentQr()
    {
        await SeedUserAsync();
        var order = DomainOrder.Create(_userId, PaymentMethod.Banking, new List<Guid>());
        await SeedOrderAsync(order);
        await SeedPaymentAsync(DomainPayment.Create(PaymentOwnerType.Store, Guid.NewGuid(), "Vietcombank", "123456789", "Nguyen Van A"));

        var result = await _handler.Handle(
            new GetOrderByIdRequest { SlugId = order.SlugId, UserId = _userId, Role = "user" },
            CancellationToken.None);

        result.PaymentQr.Should().NotBeNull();
        result.PaymentQr!.BankCode.Should().Be("Vietcombank");
        result.PaymentQr.AccountNumber.Should().Be("123456789");
        result.PaymentQr.AccountHolderName.Should().Be("Nguyen Van A");
        result.PaymentQr.Amount.Should().Be(order.TotalAmount);
        result.PaymentQr.OrderReference.Should().Be(order.SlugId);
    }

    [Fact]
    public async Task Handle_WhenOrderIsCashOnDelivery_ReturnsNullPaymentQr()
    {
        await SeedUserAsync();
        var order = DomainOrder.Create(_userId, PaymentMethod.CashOnDelivery, new List<Guid>());
        await SeedOrderAsync(order);

        var result = await _handler.Handle(
            new GetOrderByIdRequest { SlugId = order.SlugId, UserId = _userId, Role = "user" },
            CancellationToken.None);

        result.PaymentQr.Should().BeNull();
    }

    [Fact]
    public async Task Handle_WhenOrderIsBankingAndUnpaidButNoPaymentConfigured_ReturnsNullPaymentQr()
    {
        await SeedUserAsync();
        var order = DomainOrder.Create(_userId, PaymentMethod.Banking, new List<Guid>());
        await SeedOrderAsync(order);

        var result = await _handler.Handle(
            new GetOrderByIdRequest { SlugId = order.SlugId, UserId = _userId, Role = "user" },
            CancellationToken.None);

        result.PaymentQr.Should().BeNull();
    }

    [Fact]
    public async Task Handle_WhenOrderIsBankingAndPaid_ReturnsNullPaymentQr()
    {
        await SeedUserAsync();
        var order = DomainOrder.Create(_userId, PaymentMethod.Banking, new List<Guid>());
        order.MarkAsPaid(DateTime.UtcNow);
        await SeedOrderAsync(order);

        var result = await _handler.Handle(
            new GetOrderByIdRequest { SlugId = order.SlugId, UserId = _userId, Role = "user" },
            CancellationToken.None);

        result.PaymentQr.Should().BeNull();
    }

    [Fact]
    public async Task Handle_WhenOrderIsPaid_ReturnsPaymentStatusPaidAndPaidAt()
    {
        await SeedUserAsync();
        var paidAt = new DateTime(2026, 4, 1, 10, 0, 0, DateTimeKind.Utc);
        var order = DomainOrder.Create(_userId, PaymentMethod.Banking, new List<Guid>());
        order.MarkAsPaid(paidAt);
        await SeedOrderAsync(order);

        var result = await _handler.Handle(
            new GetOrderByIdRequest { SlugId = order.SlugId, UserId = _userId, Role = "user" },
            CancellationToken.None);

        result.PaymentStatus.Should().Be("paid");
        result.PaidAt.Should().Be(paidAt);
    }

    [Fact]
    public async Task Handle_WhenOrderHasPendingRefund_ReturnsRefundBlockWithPendingStatus()
    {
        await SeedUserAsync();
        var order = DomainOrder.Create(_userId, PaymentMethod.Banking, new List<Guid>());
        order.AddItem(Domain.Order.OrderItem.Create(order.Id, Guid.NewGuid(), "Burger", unitPrice: 150_000m, discountPrice: null, quantity: 1));
        order.MarkAsPaid(DateTime.UtcNow);
        order.CreateRefund();
        await SeedOrderAsync(order);

        var result = await _handler.Handle(
            new GetOrderByIdRequest { SlugId = order.SlugId, UserId = _userId, Role = "user" },
            CancellationToken.None);

        result.Refund.Should().NotBeNull();
        result.Refund!.Status.Should().Be("pending");
        result.Refund.Amount.Should().Be(order.TotalAmount);
    }

    [Fact]
    public async Task Handle_WhenRefundProcessing_CollapsesStatusToPending()
    {
        await SeedUserAsync();
        var order = DomainOrder.Create(_userId, PaymentMethod.Banking, new List<Guid>());
        order.AddItem(Domain.Order.OrderItem.Create(order.Id, Guid.NewGuid(), "Burger", unitPrice: 150_000m, discountPrice: null, quantity: 1));
        order.MarkAsPaid(DateTime.UtcNow);
        order.CreateRefund();
        order.MarkRefundProcessing();
        await SeedOrderAsync(order);

        var result = await _handler.Handle(
            new GetOrderByIdRequest { SlugId = order.SlugId, UserId = _userId, Role = "user" },
            CancellationToken.None);

        result.Refund.Should().NotBeNull();
        result.Refund!.Status.Should().Be("pending");
    }

    [Fact]
    public async Task Handle_WhenRefundFailed_CollapsesStatusToPendingAndHidesReason()
    {
        await SeedUserAsync();
        var order = DomainOrder.Create(_userId, PaymentMethod.Banking, new List<Guid>());
        order.AddItem(Domain.Order.OrderItem.Create(order.Id, Guid.NewGuid(), "Burger", unitPrice: 150_000m, discountPrice: null, quantity: 1));
        order.MarkAsPaid(DateTime.UtcNow);
        order.CreateRefund();
        order.MarkRefundFailed("provider declined");
        await SeedOrderAsync(order);

        var result = await _handler.Handle(
            new GetOrderByIdRequest { SlugId = order.SlugId, UserId = _userId, Role = "user" },
            CancellationToken.None);

        result.Refund.Should().NotBeNull();
        result.Refund!.Status.Should().Be("pending");
    }

    [Fact]
    public async Task Handle_WhenRefundPaid_ReturnsPaidStatusWithTransferReferenceAndSentDate()
    {
        await SeedUserAsync();
        var order = DomainOrder.Create(_userId, PaymentMethod.Banking, new List<Guid>());
        order.AddItem(Domain.Order.OrderItem.Create(order.Id, Guid.NewGuid(), "Burger", unitPrice: 150_000m, discountPrice: null, quantity: 1));
        order.MarkAsPaid(DateTime.UtcNow);
        order.CreateRefund();
        order.MarkRefundProcessing();
        order.MarkRefundPaid("SEPAY-TX-PAID");
        await SeedOrderAsync(order);

        var result = await _handler.Handle(
            new GetOrderByIdRequest { SlugId = order.SlugId, UserId = _userId, Role = "user" },
            CancellationToken.None);

        result.Refund.Should().NotBeNull();
        result.Refund!.Status.Should().Be("paid");
        result.Refund.TransferReference.Should().Be("SEPAY-TX-PAID");
        result.Refund.SentDate.Should().NotBeNull();
    }

    [Fact]
    public async Task Handle_WhenRefundPending_DoesNotExposeTransferReferenceOrSentDate()
    {
        await SeedUserAsync();
        var order = DomainOrder.Create(_userId, PaymentMethod.Banking, new List<Guid>());
        order.AddItem(Domain.Order.OrderItem.Create(order.Id, Guid.NewGuid(), "Burger", unitPrice: 150_000m, discountPrice: null, quantity: 1));
        order.MarkAsPaid(DateTime.UtcNow);
        order.CreateRefund();
        await SeedOrderAsync(order);

        var result = await _handler.Handle(
            new GetOrderByIdRequest { SlugId = order.SlugId, UserId = _userId, Role = "user" },
            CancellationToken.None);

        result.Refund.Should().NotBeNull();
        result.Refund!.TransferReference.Should().BeNull();
        result.Refund.SentDate.Should().BeNull();
    }

    [Fact]
    public async Task Handle_WhenRefundFailed_HidesTransferReferenceAndSentDate()
    {
        await SeedUserAsync();
        var order = DomainOrder.Create(_userId, PaymentMethod.Banking, new List<Guid>());
        order.AddItem(Domain.Order.OrderItem.Create(order.Id, Guid.NewGuid(), "Burger", unitPrice: 150_000m, discountPrice: null, quantity: 1));
        order.MarkAsPaid(DateTime.UtcNow);
        order.CreateRefund();
        order.MarkRefundProcessing();
        order.MarkRefundFailed("provider declined");
        await SeedOrderAsync(order);

        var result = await _handler.Handle(
            new GetOrderByIdRequest { SlugId = order.SlugId, UserId = _userId, Role = "user" },
            CancellationToken.None);

        result.Refund.Should().NotBeNull();
        result.Refund!.Status.Should().Be("pending");
        result.Refund.TransferReference.Should().BeNull();
        result.Refund.SentDate.Should().BeNull();
    }

    [Fact]
    public async Task Handle_WhenOrderHasNoRefund_ReturnsNullRefund()
    {
        await SeedUserAsync();
        var order = DomainOrder.Create(_userId, PaymentMethod.Banking, new List<Guid>());
        await SeedOrderAsync(order);

        var result = await _handler.Handle(
            new GetOrderByIdRequest { SlugId = order.SlugId, UserId = _userId, Role = "user" },
            CancellationToken.None);

        result.Refund.Should().BeNull();
    }

    [Fact]
    public async Task Handle_WhenOrderNotFound_ThrowsResultFailureException()
    {
        await SeedUserAsync();

        var act = async () => await _handler.Handle(
            new GetOrderByIdRequest { SlugId = "ORD-MISSING", UserId = _userId, Role = "user" },
            CancellationToken.None);

        await act.Should().ThrowAsync<ResultFailureException>();
    }

    [Fact]
    public async Task Handle_WhenPaymentQrBuilt_OrderReferenceMatchesOrderSlugId()
    {
        await SeedUserAsync();
        var order = DomainOrder.Create(_userId, PaymentMethod.Banking, new List<Guid>());
        await SeedOrderAsync(order);
        await SeedPaymentAsync(DomainPayment.Create(PaymentOwnerType.Store, Guid.NewGuid(), "MB", "9999", "Le Van C"));

        var result = await _handler.Handle(
            new GetOrderByIdRequest { SlugId = order.SlugId, UserId = _userId, Role = "user" },
            CancellationToken.None);

        result.PaymentQr!.OrderReference.Should().Be(order.SlugId);
        result.SlugId.Should().Be(order.SlugId);
    }

    [Fact]
    public async Task Handle_WhenCustomerRowPresent_StillResolvesStoreRowForCheckoutQr()
    {
        await SeedUserAsync();
        var order = DomainOrder.Create(_userId, PaymentMethod.Banking, new List<Guid>());
        await SeedOrderAsync(order);

        // Customer payout row for the order owner must never satisfy the checkout QR.
        await SeedPaymentAsync(DomainPayment.Create(PaymentOwnerType.Customer, _userId, "CustomerBank", "000", "Customer"));
        await SeedPaymentAsync(DomainPayment.Create(PaymentOwnerType.Store, Guid.NewGuid(), "Vietcombank", "123456789", "Store Account"));

        var result = await _handler.Handle(
            new GetOrderByIdRequest { SlugId = order.SlugId, UserId = _userId, Role = "user" },
            CancellationToken.None);

        result.PaymentQr.Should().NotBeNull();
        result.PaymentQr!.BankCode.Should().Be("Vietcombank");
        result.PaymentQr.AccountNumber.Should().Be("123456789");
        result.PaymentQr.AccountHolderName.Should().Be("Store Account");
    }

    [Fact]
    public async Task Handle_WhenOnlyCustomerRowPresent_ReturnsNullPaymentQr()
    {
        await SeedUserAsync();
        var order = DomainOrder.Create(_userId, PaymentMethod.Banking, new List<Guid>());
        await SeedOrderAsync(order);
        await SeedPaymentAsync(DomainPayment.Create(PaymentOwnerType.Customer, _userId, "CustomerBank", "000", "Customer"));

        var result = await _handler.Handle(
            new GetOrderByIdRequest { SlugId = order.SlugId, UserId = _userId, Role = "user" },
            CancellationToken.None);

        result.PaymentQr.Should().BeNull();
    }

    [Fact]
    public async Task Handle_WhenRefundAndCallerHasCustomerPayout_ReturnsHasBankDetailsTrue()
    {
        await SeedUserAsync();
        var order = DomainOrder.Create(_userId, PaymentMethod.Banking, new List<Guid>());
        order.AddItem(Domain.Order.OrderItem.Create(order.Id, Guid.NewGuid(), "Burger", unitPrice: 150_000m, discountPrice: null, quantity: 1));
        order.MarkAsPaid(DateTime.UtcNow);
        order.CreateRefund();
        await SeedOrderAsync(order);
        await SeedPaymentAsync(DomainPayment.Create(PaymentOwnerType.Customer, _userId, "Techcombank", "987", "Buyer"));

        var result = await _handler.Handle(
            new GetOrderByIdRequest { SlugId = order.SlugId, UserId = _userId, Role = "user" },
            CancellationToken.None);

        result.Refund.Should().NotBeNull();
        result.Refund!.HasBankDetails.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WhenRefundAndCallerHasNoCustomerPayout_ReturnsHasBankDetailsFalse()
    {
        await SeedUserAsync();
        var order = DomainOrder.Create(_userId, PaymentMethod.Banking, new List<Guid>());
        order.AddItem(Domain.Order.OrderItem.Create(order.Id, Guid.NewGuid(), "Burger", unitPrice: 150_000m, discountPrice: null, quantity: 1));
        order.MarkAsPaid(DateTime.UtcNow);
        order.CreateRefund();
        await SeedOrderAsync(order);

        // Only a Store row exists — it must not count as the caller's payout details.
        await SeedPaymentAsync(DomainPayment.Create(PaymentOwnerType.Store, Guid.NewGuid(), "Vietcombank", "111", "Store"));

        var result = await _handler.Handle(
            new GetOrderByIdRequest { SlugId = order.SlugId, UserId = _userId, Role = "user" },
            CancellationToken.None);

        result.Refund.Should().NotBeNull();
        result.Refund!.HasBankDetails.Should().BeFalse();
    }

    private async Task SeedUserAsync()
    {
        var user = Domain.User.User.Create("orderbyid@example.com", "hashedpassword", UserRole.User);
        user.ClearDomainEvents();
        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();
        _userId = user.Id;
        _dbContext.ChangeTracker.Clear();
    }

    private async Task SeedOrderAsync(DomainOrder order)
    {
        order.ClearDomainEvents();
        _dbContext.Orders.Add(order);
        await _dbContext.SaveChangesAsync();
        _dbContext.ChangeTracker.Clear();
    }

    private async Task SeedPaymentAsync(DomainPayment payment)
    {
        payment.ClearDomainEvents();
        _dbContext.Payments.Add(payment);
        await _dbContext.SaveChangesAsync();
        _dbContext.ChangeTracker.Clear();
    }
}