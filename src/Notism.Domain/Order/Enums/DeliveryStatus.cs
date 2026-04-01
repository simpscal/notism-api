using Notism.Shared.Attributes;

namespace Notism.Domain.Order.Enums;

public enum DeliveryStatus
{
    [StringValue("orderPlaced")]
    OrderPlaced,

    [StringValue("preparing")]
    Preparing,

    [StringValue("onTheWay")]
    OnTheWay,

    [StringValue("delivered")]
    Delivered,

    [StringValue("cancelled")]
    Cancelled,
}