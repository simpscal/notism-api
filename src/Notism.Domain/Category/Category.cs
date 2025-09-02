using System.ComponentModel;

using Notism.Domain.Common;

namespace Notism.Domain.Category;

public class Category : Entity
{
    [Description("CategoryType")]
    public required string Name { get; set; }
    public string? Description { get; set; }
}