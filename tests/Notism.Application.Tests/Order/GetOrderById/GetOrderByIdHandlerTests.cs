using FluentAssertions;

using Microsoft.Extensions.Logging;

using Notism.Application.Common.Interfaces;
using Notism.Application.Common.Services;
using Notism.Application.Order.GetOrderById;
using Notism.Domain.Common.Specifications;
using Notism.Domain.Order;
using Notism.Domain.Order.Enums;
using Notism.Domain.Payment;
using Notism.Shared.Exceptions;

using NSubstitute;

namespace Notism.Application.Tests.Order.GetOrderById;

public class GetOrderByIdHandlerTests
{
    private readonly IOrderRepository _orderRepository;
    private readonly IStorageService _storageService;
    private readonly ILogger<GetOrderByIdHandler> _logger;
    private readonly IMessages _messages;
    private readonly IPaymentRepository _paymentRepository;
    private readonly GetOrderByIdHandler _handler;

    public GetOrderByIdHandlerTests()
    {
        _orderRepository = Substitute.For<IOrderRepository>();
        _storageService = Substitute.For<IStorageService>();
        _logger = Substitute.For<ILogger<GetOrderByIdHandler>>();
        _messages = Substitute.For<IMessages>();
        _paymentRepository = Substitute.For<IPaymentRepository>();

        _messages.OrderNotFound.Returns("Order not found.");

        _handler = new GetOrderByIdHandler(
            _orderRepository,
            _storageService,
            _logger,
            _messages,
            _paymentRepository);
    }

    [Fact]
    public async Task Handle_WhenOrderIsBankingAndUnpaidAndPaymentExists_ReturnsPaymentQr()
    {
        var userId = Guid.NewGuid();
        var order = Domain.Order.Order.Create(userId, PaymentMethod.Banking, new List<Guid>());
        var storerId = Guid.NewGuid();
        var payment = Domain.Payment.Payment.Create(storerId, "Vietcombank", "123456789", "Nguyen Van A");

        _orderRepository
            .FindByExpressionAsync(Arg.Any<FilterSpecification<Domain.Order.Order>>())
            .Returns(order);

        _paymentRepository
            .FindByExpressionAsync(Arg.Any<FilterSpecification<Domain.Payment.Payment>>())
            .Returns(payment);

        var request = new GetOrderByIdRequest { SlugId = order.SlugId, UserId = userId, Role = "user" };
        var result = await _handler.Handle(request, CancellationToken.None);

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
        var userId = Guid.NewGuid();
        var order = Domain.Order.Order.Create(userId, PaymentMethod.CashOnDelivery, new List<Guid>());

        _orderRepository
            .FindByExpressionAsync(Arg.Any<FilterSpecification<Domain.Order.Order>>())
            .Returns(order);

        _paymentRepository
            .FindByExpressionAsync(Arg.Any<FilterSpecification<Domain.Payment.Payment>>())
            .Returns((Domain.Payment.Payment?)null);

        var request = new GetOrderByIdRequest { SlugId = order.SlugId, UserId = userId, Role = "user" };
        var result = await _handler.Handle(request, CancellationToken.None);

        result.PaymentQr.Should().BeNull();
        result.BankAccountConfigured.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_WhenOrderIsBankingAndUnpaidButNoPaymentConfigured_ReturnsNullPaymentQr()
    {
        var userId = Guid.NewGuid();
        var order = Domain.Order.Order.Create(userId, PaymentMethod.Banking, new List<Guid>());

        _orderRepository
            .FindByExpressionAsync(Arg.Any<FilterSpecification<Domain.Order.Order>>())
            .Returns(order);

        _paymentRepository
            .FindByExpressionAsync(Arg.Any<FilterSpecification<Domain.Payment.Payment>>())
            .Returns((Domain.Payment.Payment?)null);

        var request = new GetOrderByIdRequest { SlugId = order.SlugId, UserId = userId, Role = "user" };
        var result = await _handler.Handle(request, CancellationToken.None);

        result.PaymentQr.Should().BeNull();
    }

    [Fact]
    public async Task Handle_WhenOrderIsBankingAndPaid_ReturnsNullPaymentQr()
    {
        var userId = Guid.NewGuid();
        var order = Domain.Order.Order.Create(userId, PaymentMethod.Banking, new List<Guid>());
        order.MarkAsPaid(DateTime.UtcNow);

        _orderRepository
            .FindByExpressionAsync(Arg.Any<FilterSpecification<Domain.Order.Order>>())
            .Returns(order);

        _paymentRepository
            .FindByExpressionAsync(Arg.Any<FilterSpecification<Domain.Payment.Payment>>())
            .Returns((Domain.Payment.Payment?)null);

        var request = new GetOrderByIdRequest { SlugId = order.SlugId, UserId = userId, Role = "user" };
        var result = await _handler.Handle(request, CancellationToken.None);

        result.PaymentQr.Should().BeNull();
        result.BankAccountConfigured.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_WhenOrderIsPaid_ReturnsPaymentStatusPaidAndPaidAt()
    {
        var userId = Guid.NewGuid();
        var paidAt = new DateTime(2026, 4, 1, 10, 0, 0, DateTimeKind.Utc);
        var order = Domain.Order.Order.Create(userId, PaymentMethod.Banking, new List<Guid>());
        order.MarkAsPaid(paidAt);

        _orderRepository
            .FindByExpressionAsync(Arg.Any<FilterSpecification<Domain.Order.Order>>())
            .Returns(order);

        var request = new GetOrderByIdRequest { SlugId = order.SlugId, UserId = userId, Role = "user" };
        var result = await _handler.Handle(request, CancellationToken.None);

        result.PaymentStatus.Should().Be("paid");
        result.PaidAt.Should().Be(paidAt);
    }

    [Fact]
    public async Task Handle_WhenOrderNotFound_ThrowsResultFailureException()
    {
        _orderRepository
            .FindByExpressionAsync(Arg.Any<FilterSpecification<Domain.Order.Order>>())
            .Returns((Domain.Order.Order?)null);

        var request = new GetOrderByIdRequest { SlugId = "ORD-MISSING", UserId = Guid.NewGuid(), Role = "user" };

        var act = async () => await _handler.Handle(request, CancellationToken.None);

        await act.Should().ThrowAsync<ResultFailureException>();
    }

    [Fact]
    public async Task Handle_WhenPaymentQrBuilt_OrderReferenceMatchesOrderSlugId()
    {
        var userId = Guid.NewGuid();
        var order = Domain.Order.Order.Create(userId, PaymentMethod.Banking, new List<Guid>());
        var storerId = Guid.NewGuid();
        var payment = Domain.Payment.Payment.Create(storerId, "MB", "9999", "Le Van C");

        _orderRepository
            .FindByExpressionAsync(Arg.Any<FilterSpecification<Domain.Order.Order>>())
            .Returns(order);

        _paymentRepository
            .FindByExpressionAsync(Arg.Any<FilterSpecification<Domain.Payment.Payment>>())
            .Returns(payment);

        var request = new GetOrderByIdRequest { SlugId = order.SlugId, UserId = userId, Role = "user" };
        var result = await _handler.Handle(request, CancellationToken.None);

        result.PaymentQr!.OrderReference.Should().Be(order.SlugId);
        result.SlugId.Should().Be(order.SlugId);
    }

    [Fact]
    public async Task Handle_WhenPaymentExists_ReturnsBankAccountConfiguredTrue()
    {
        var userId = Guid.NewGuid();
        var order = Domain.Order.Order.Create(userId, PaymentMethod.Banking, new List<Guid>());
        var storerId = Guid.NewGuid();
        var payment = Domain.Payment.Payment.Create(storerId, "Vietcombank", "123456789", "Nguyen Van A");

        _orderRepository
            .FindByExpressionAsync(Arg.Any<FilterSpecification<Domain.Order.Order>>())
            .Returns(order);

        _paymentRepository
            .FindByExpressionAsync(Arg.Any<FilterSpecification<Domain.Payment.Payment>>())
            .Returns(payment);

        var request = new GetOrderByIdRequest { SlugId = order.SlugId, UserId = userId, Role = "user" };
        var result = await _handler.Handle(request, CancellationToken.None);

        result.BankAccountConfigured.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WhenNoPaymentExists_ReturnsBankAccountConfiguredFalse()
    {
        var userId = Guid.NewGuid();
        var order = Domain.Order.Order.Create(userId, PaymentMethod.CashOnDelivery, new List<Guid>());

        _orderRepository
            .FindByExpressionAsync(Arg.Any<FilterSpecification<Domain.Order.Order>>())
            .Returns(order);

        _paymentRepository
            .FindByExpressionAsync(Arg.Any<FilterSpecification<Domain.Payment.Payment>>())
            .Returns((Domain.Payment.Payment?)null);

        var request = new GetOrderByIdRequest { SlugId = order.SlugId, UserId = userId, Role = "user" };
        var result = await _handler.Handle(request, CancellationToken.None);

        result.BankAccountConfigured.Should().BeFalse();
    }
}
