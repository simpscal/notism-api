namespace Notism.Api.Endpoints;

public record AdminUpdateCustomisationGroupPayload
{
    public string? Label { get; set; }
    public bool? IsRequired { get; set; }
    public int? DisplayOrder { get; set; }
}