namespace Notism.Api.Endpoints;

public record AddCartItemPayload
{
    public Guid FoodId { get; set; }
    public int Quantity { get; set; }
    public List<CartItemCustomisationSelectionPayload>? Customisations { get; set; }
}