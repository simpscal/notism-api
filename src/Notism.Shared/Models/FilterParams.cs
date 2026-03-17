namespace Notism.Shared.Models;

public record FilterParams : Pagination
{
    public string? Keyword { get; set; }
    public string? SortBy { get; set; }
    public string? SortOrder { get; set; }
}