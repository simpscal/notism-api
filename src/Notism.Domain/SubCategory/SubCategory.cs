using System.ComponentModel;

using Notism.Domain.Common;
using Notism.Domain.Common.Enums;

namespace Notism.Domain.SubCategory;

public class SubCategory : Entity
{
    [Description("SubCategoryType")]
    public required string Name { get; set; }
}