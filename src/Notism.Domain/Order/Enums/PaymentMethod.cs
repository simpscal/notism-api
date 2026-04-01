using Notism.Shared.Attributes;

namespace Notism.Domain.Order.Enums;

public enum PaymentMethod
{
    [StringValue("cashOnDelivery")]
    CashOnDelivery,

    [StringValue("banking")]
    Banking,
}