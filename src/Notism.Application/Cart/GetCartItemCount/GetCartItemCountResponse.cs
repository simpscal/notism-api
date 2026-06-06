namespace Notism.Application.Cart.GetCartItemCount;

public sealed record GetCartItemCountResponse
{
    public int TotalQuantity { get; set; }
    public int ItemCount { get; set; }
}