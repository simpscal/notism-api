using MediatR;

namespace Notism.Application.Payment.GetBankAccount;

public class GetBankAccountRequest : IRequest<GetBankAccountResponse?>
{
    /// <summary>
    /// Set by admin callers — retrieves their own bank account record.
    /// </summary>
    public Guid? StorerId { get; set; }

    /// <summary>
    /// Set by consumer callers — scopes access to the caller's pending checkout.
    /// </summary>
    public Guid? CheckoutId { get; set; }

    /// <summary>
    /// The authenticated user's ID. Required when <see cref="CheckoutId"/> is provided
    /// so the handler can verify the checkout belongs to the requesting consumer.
    /// </summary>
    public Guid? UserId { get; set; }
}
