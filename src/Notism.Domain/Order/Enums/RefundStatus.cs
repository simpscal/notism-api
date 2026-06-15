using Notism.Shared.Attributes;

namespace Notism.Domain.Order.Enums;

public enum RefundStatus
{
    [StringValue("pending")]
    Pending,

    [StringValue("processing")]
    Processing,

    [StringValue("paid")]
    Paid,

    [StringValue("failed")]
    Failed,
}