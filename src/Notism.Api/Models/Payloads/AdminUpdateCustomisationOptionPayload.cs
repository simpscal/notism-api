namespace Notism.Api.Endpoints;

public record AdminUpdateCustomisationOptionPayload
{
    public string? Label { get; set; }
    public decimal? Surcharge { get; set; }
    public int? DisplayOrder { get; set; }
}