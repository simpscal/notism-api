using System.ComponentModel;

using Order.Domain.Common;

namespace Order.Domain.ProductSize;

public class ProductSize : Entity
{
    [Description("SizeType")]
    public string Name { get; set; } = string.Empty;

    public ICollection<Product.Product> Products { get; set; } = [];
}