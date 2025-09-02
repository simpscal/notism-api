using System.ComponentModel;

using Notism.Domain.Common;

namespace Notism.Domain.ProductSize;

public class ProductSize : Entity
{
    [Description("SizeType")]
    public string Name { get; set; } = string.Empty;

    public ICollection<Product.Product> Products { get; set; } = [];
}