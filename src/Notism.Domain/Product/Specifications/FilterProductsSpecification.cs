using System.Linq.Expressions;

using Notism.Domain.Common.Specifications;
using Notism.Domain.Product.Models;

namespace Notism.Domain.Product.Specifications;

public class FilterProductsSpecification(ProductFilterParams filterParams) : Specification<Product>
{
    public override Expression<Func<Product, bool>> ToExpression()
    {
        return product =>
            (filterParams.Price == null || product.Price == filterParams.Price) &&
            product.Name.Contains(filterParams.Keyword ?? string.Empty);
    }
}