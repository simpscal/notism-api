using Notism.Shared.Models;

namespace Notism.Domain.Product.Models;

public record ProductFilterParams : FilterParams
{
    public decimal? Price { get; set; }
}