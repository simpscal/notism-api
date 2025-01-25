namespace Order.Shared.Models;

public record Pagination
{
    public int Skip { get; set; } = 0;
    public int Take { get; set; } = 25;
}

public record PagedResult<T>
{
    public int TotalCount { get; set; } = 0;
    public required IEnumerable<T> Items { get; set; }
}