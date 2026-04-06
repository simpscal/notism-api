namespace Notism.Application.Order.Models;

public class PaymentQrResponse
{
    public string BankCode { get; set; } = string.Empty;
    public string AccountNumber { get; set; } = string.Empty;
    public string AccountHolderName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string OrderReference { get; set; } = string.Empty;
}
