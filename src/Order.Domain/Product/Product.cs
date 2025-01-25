using Order.Domain.Common;
using Order.Domain.Common.Enums;

namespace Order.Domain.Product;

public class Product : Entity
{
    public required string Name { get; set; }
    public required decimal Price { get; set; }
    public string[] Images { get; set; } = [];

    public required ICollection<ProductColor.ProductColor> ProductColors { get; set; }
    public required ICollection<ProductSize.ProductSize> ProductSizes { get; set; }

    public required Guid SubCategoryId { get; set; }
    public SubCategory.SubCategory? SubCategory { get; set; }

    public required Guid CategoryId { get; set; }
    public Category.Category? Category { get; set; }
}