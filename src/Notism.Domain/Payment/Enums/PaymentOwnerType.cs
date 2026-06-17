using Notism.Shared.Attributes;

namespace Notism.Domain.Payment.Enums;

public enum PaymentOwnerType
{
    [StringValue("store")]
    Store = 0,

    [StringValue("customer")]
    Customer = 1,
}