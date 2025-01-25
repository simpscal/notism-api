using System.ComponentModel;

using Order.Domain.Common;
using Order.Domain.Common.Enums;

namespace Order.Domain.SubCategory;

public class SubCategory : Entity
{
    [Description("SubCategoryType")]
    public required string Name { get; set; }
}