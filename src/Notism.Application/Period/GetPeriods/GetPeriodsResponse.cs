namespace Notism.Application.Period.GetPeriods;

public class GetPeriodsResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int StartYear { get; set; }
    public int EndYear { get; set; }
    public string? Description { get; set; }
    public string? ThumbnailUrl { get; set; }
    public int DisplayOrder { get; set; }
}

