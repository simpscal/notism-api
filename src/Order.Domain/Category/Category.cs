using System.ComponentModel;

using Order.Domain.Common;

namespace Order.Domain.Category;

public class Category : Entity
{
    [Description("CategoryType")]
    public required string Name { get; set; }
    public string? Description { get; set; }
}