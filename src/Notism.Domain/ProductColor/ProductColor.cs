using System.ComponentModel;

using Notism.Domain.Common;

namespace Notism.Domain.ProductColor;

public class ProductColor : Entity
{
    [Description("ColorType")]
    public string Name { get; set; } = string.Empty;

    public string Code { get; set; } = string.Empty;

    public ICollection<Product.Product> Products { get; set; } = [];
}