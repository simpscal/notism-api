namespace Notism.Api.Endpoints;

public record AdminAddCustomisationOptionPayload
{
    public string Label { get; set; } = string.Empty;
    public decimal? Surcharge { get; set; }
    public int DisplayOrder { get; set; }
}