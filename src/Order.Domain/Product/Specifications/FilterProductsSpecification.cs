using System.Linq.Expressions;

using Order.Domain.Common.Specifications;
using Order.Domain.Product.Models;

namespace Order.Domain.Product.Specifications;

public class FilterProductsSpecification(ProductFilterParams filterParams) : Specification<Product>
{
    public override Expression<Func<Product, bool>> ToExpression()
    {
        return product =>
            (filterParams.Price == null || product.Price == filterParams.Price) &&
            product.Name.Contains(filterParams.Keyword ?? string.Empty);
    }
}