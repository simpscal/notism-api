namespace Order.Shared.Models;

public record FilterParams : Pagination
{
    public string? Keyword { get; set; }
}