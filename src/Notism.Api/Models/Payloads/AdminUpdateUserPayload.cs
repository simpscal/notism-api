namespace Notism.Api.Endpoints;

public record AdminUpdateUserPayload
{
    public string Role { get; set; } = string.Empty;
}