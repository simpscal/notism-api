using Notism.Shared.Attributes;

namespace Notism.Domain.User.Enums;

public enum BankAccountOwnerType
{
    [StringValue("store")]
    Store = 0,

    [StringValue("customer")]
    Customer = 1,
}
