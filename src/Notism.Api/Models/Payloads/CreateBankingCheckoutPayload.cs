namespace Notism.Api.Endpoints;

public record CreateBankingCheckoutPayload
{
    public List<Guid> CartItemIds { get; set; } = new();
    public decimal TotalAmount { get; set; }
}