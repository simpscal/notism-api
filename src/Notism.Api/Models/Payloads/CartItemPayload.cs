namespace Notism.Api.Endpoints;

public record CartItemPayload
{
    public Guid FoodId { get; set; }
    public int Quantity { get; set; }
}