using Notism.Shared.Attributes;

namespace Notism.Domain.Order.Enums;

public enum PaymentStatus
{
    [StringValue("unpaid")]
    Unpaid = 0,

    [StringValue("paid")]
    Paid = 1,

    [StringValue("failed")]
    Failed = 2,

    [StringValue("refunded")]
    Refunded = 3,
}