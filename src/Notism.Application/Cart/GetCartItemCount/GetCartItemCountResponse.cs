namespace Notism.Application.Cart.GetCartItemCount;

public record GetCartItemCountResponse
{
    public int TotalQuantity { get; set; }
    public int ItemCount { get; set; }
}