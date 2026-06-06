namespace Notism.Api.Endpoints;

public record AdminAddCustomisationGroupPayload
{
    public string Label { get; set; } = string.Empty;
    public bool IsRequired { get; set; }
    public int DisplayOrder { get; set; }
}