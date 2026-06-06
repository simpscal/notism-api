namespace Notism.Api.Endpoints;

public record UpdateCartItemQuantityPayload
{
    public int Quantity { get; set; }
}