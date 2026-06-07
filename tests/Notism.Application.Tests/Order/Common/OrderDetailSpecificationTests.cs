using FluentAssertions;

using Notism.Application.Order.Common;
using Notism.Domain.Order.Enums;
using Notism.Domain.Payment.Enums;

namespace Notism.Application.Tests.Order.Common;

public class OrderDetailSpecificationTests
{
    [Fact]
    public void ApplyOrdering_OrdersByCreatedAtDescendingThenIdDescending()
    {
        var userId = Guid.NewGuid();

        var sharedCreatedAt = new DateTime(2026, 5, 1, 12, 0, 0, DateTimeKind.Utc);
        var earlier = CreateOrderWith(userId, new DateTime(2026, 4, 1, 12, 0, 0, DateTimeKind.Utc), Guid.Parse("00000000-0000-0000-0000-000000000001"));
        var tieLowerId = CreateOrderWith(userId, sharedCreatedAt, Guid.Parse("11111111-1111-1111-1111-111111111111"));
        var tieHigherId = CreateOrderWith(userId, sharedCreatedAt, Guid.Parse("22222222-2222-2222-2222-222222222222"));

        var specification = new OrderDetailSpecification(o => o.UserId == userId);

        var ordered = specification
            .ApplyOrdering(new[] { earlier, tieLowerId, tieHigherId }.AsQueryable())
            .ToList();

        ordered.Should().ContainInOrder(tieHigherId, tieLowerId, earlier);
    }

    [Fact]
    public void ToExpression_ReturnsCallerFilterVerbatim()
    {
        var userId = Guid.NewGuid();
        var paidOrder = CreatePaidOrder(userId);
        var unpaidOrder = Domain.Order.Order.Create(userId, PaymentMethod.Banking, new List<Guid>());

        var specification = new OrderDetailSpecification(o => o.UserId == userId);

        specification.IsSatisfiedBy(paidOrder).Should().BeTrue();
        specification.IsSatisfiedBy(unpaidOrder).Should().BeTrue();
    }

    [Fact]
    public void ToExpression_WithComposedPaymentStatusFilter_MatchesOnlyOrdersSatisfyingAllConditions()
    {
        var userId = Guid.NewGuid();
        var paidOrder = CreatePaidOrder(userId);
        var unpaidOrder = Domain.Order.Order.Create(userId, PaymentMethod.Banking, new List<Guid>());
        var otherUsersPaidOrder = CreatePaidOrder(Guid.NewGuid());

        var specification = new OrderDetailSpecification(
            o => o.UserId == userId && o.PaymentStatus == PaymentStatus.Paid);

        specification.IsSatisfiedBy(paidOrder).Should().BeTrue();
        specification.IsSatisfiedBy(unpaidOrder).Should().BeFalse();
        specification.IsSatisfiedBy(otherUsersPaidOrder).Should().BeFalse();
    }

    private static Domain.Order.Order CreatePaidOrder(Guid userId)
    {
        var order = Domain.Order.Order.Create(userId, PaymentMethod.Banking, new List<Guid>());
        order.MarkAsPaid(DateTime.UtcNow);
        return order;
    }

    private static Domain.Order.Order CreateOrderWith(Guid userId, DateTime createdAt, Guid id)
    {
        var order = Domain.Order.Order.Create(userId, PaymentMethod.Banking, new List<Guid>());
        SetProperty(order, "Id", id);
        SetProperty(order, "CreatedAt", createdAt);
        return order;
    }

    private static void SetProperty(object target, string propertyName, object value)
    {
        var property = target.GetType().GetProperty(propertyName)
            ?? throw new InvalidOperationException($"Property {propertyName} not found");
        property.SetValue(target, value);
    }
}