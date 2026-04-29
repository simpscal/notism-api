using Notism.Shared.Attributes;

namespace Notism.Domain.Payment.Enums;

public enum PaymentStatus
{
    [StringValue("unpaid")]
    Unpaid = 0,

    [StringValue("paid")]
    Paid = 1,

    [StringValue("failed")]
    Failed = 2,
}
