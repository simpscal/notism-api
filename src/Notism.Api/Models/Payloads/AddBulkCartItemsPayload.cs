namespace Notism.Api.Endpoints;

public record AddBulkCartItemsPayload
{
    public List<CartItemPayload> Items { get; set; } = new();
}