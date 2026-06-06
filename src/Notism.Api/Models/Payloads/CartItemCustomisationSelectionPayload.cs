namespace Notism.Api.Endpoints;

public record CartItemCustomisationSelectionPayload
{
    public Guid GroupId { get; set; }
    public Guid OptionId { get; set; }
}