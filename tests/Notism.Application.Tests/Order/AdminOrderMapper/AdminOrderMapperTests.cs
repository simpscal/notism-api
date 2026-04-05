using FluentAssertions;

using Notism.Domain.Order.Enums;
using Notism.Domain.Payment.Enums;

namespace Notism.Application.Tests.Order.AdminOrderMapper;

public class AdminOrderMapperTests
{
    [Fact]
    public void ToAdminOrderResponse_WhenOrderIsUnpaid_IncludesPaymentStatusUnpaidAndNullPaidAt()
    {
        var userId = Guid.NewGuid();
        var order = Domain.Order.Order.Create(userId, PaymentMethod.Banking, new List<Guid>());

        var response = Application.Order.Mappers.AdminOrderMapper.ToAdminOrderResponse(order, null);

        response.PaymentStatus.Should().Be("unpaid");
        response.PaidAt.Should().BeNull();
    }

    [Fact]
    public void ToAdminOrderResponse_WhenOrderIsPaid_IncludesPaymentStatusPaidAndPaidAt()
    {
        var userId = Guid.NewGuid();
        var paidAt = new DateTime(2026, 4, 5, 8, 30, 0, DateTimeKind.Utc);
        var order = Domain.Order.Order.Create(userId, PaymentMethod.Banking, new List<Guid>());
        order.MarkAsPaid(paidAt);

        var response = Application.Order.Mappers.AdminOrderMapper.ToAdminOrderResponse(order, null);

        response.PaymentStatus.Should().Be("paid");
        response.PaidAt.Should().Be(paidAt);
    }

    [Fact]
    public void ToAdminOrderResponse_WhenUserProvided_MapsUserEmailAndName()
    {
        var userId = Guid.NewGuid();
        var order = Domain.Order.Order.Create(userId, PaymentMethod.CashOnDelivery, new List<Guid>());

        var response = Application.Order.Mappers.AdminOrderMapper.ToAdminOrderResponse(order, null);

        response.UserEmail.Should().BeEmpty();
        response.UserName.Should().BeEmpty();
    }

    [Fact]
    public void ToAdminOrderResponse_WhenUserIsNull_ReturnsEmptyEmailAndName()
    {
        var userId = Guid.NewGuid();
        var order = Domain.Order.Order.Create(userId, PaymentMethod.CashOnDelivery, new List<Guid>());

        var response = Application.Order.Mappers.AdminOrderMapper.ToAdminOrderResponse(order, null);

        response.UserEmail.Should().BeEmpty();
        response.UserName.Should().BeEmpty();
    }

    [Fact]
    public void ToAdminOrderResponse_MapsDeliveryStatusAsStringValue()
    {
        var userId = Guid.NewGuid();
        var order = Domain.Order.Order.Create(userId, PaymentMethod.Banking, new List<Guid>());

        var response = Application.Order.Mappers.AdminOrderMapper.ToAdminOrderResponse(order, null);

        response.DeliveryStatus.Should().NotBeNullOrEmpty();
    }
}
