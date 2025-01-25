using Order.Shared.Models;

namespace Order.Domain.Product.Models;

public record ProductFilterParams : FilterParams
{
    public decimal? Price { get; set; }
}