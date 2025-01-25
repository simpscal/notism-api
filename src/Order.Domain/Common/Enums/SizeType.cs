using Order.Shared.Attributes;

namespace Order.Domain.Common.Enums;

public enum SizeType
{
    [StringValue("xs")]
    Xs,
    [StringValue("s")]
    S,
    [StringValue("m")]
    M,
    [StringValue("xl")]
    L,
    [StringValue("xl")]
    Xl,
    [StringValue("xl2")]
    Xl2,
    [StringValue("xl3")]
    Xl3,
}