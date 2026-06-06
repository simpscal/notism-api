using System.Linq.Expressions;

using Notism.Domain.Common.Specifications;

using DomainPayment = Notism.Domain.Payment.Payment;

namespace Notism.Application.Payment.Common;

/// <summary>
/// Resolves the store's configured bank account (single-row lookup).
/// Repository <c>FindByExpressionAsync</c> applies <c>FirstOrDefault</c>, so this
/// returns the configured account without loading the whole table.
/// </summary>
public class BankAccountSpecification : Specification<DomainPayment>
{
    public override Expression<Func<DomainPayment, bool>> ToExpression()
    {
        return _ => true;
    }
}