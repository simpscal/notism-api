using MediatR;

namespace Notism.Application.Payment.CreateBankingCheckout;

public class CreateBankingCheckoutRequest : IRequest<CreateBankingCheckoutResponse>
{
    public Guid UserId { get; set; }
    public List<Guid> CartItemIds { get; set; } = new();
    public decimal TotalAmount { get; set; }
}
