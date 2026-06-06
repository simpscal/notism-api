namespace Notism.Api.Endpoints;

public record AdminUpdateCategoryPayload
{
    public string Name { get; set; } = string.Empty;
}