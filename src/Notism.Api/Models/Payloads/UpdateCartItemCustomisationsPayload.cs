namespace Notism.Api.Endpoints;

public record UpdateCartItemCustomisationsPayload
{
    public List<CartItemCustomisationSelectionPayload> Customisations { get; set; } = new();
}