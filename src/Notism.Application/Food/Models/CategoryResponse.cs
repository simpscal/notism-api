namespace Notism.Application.Food.Models;

public record CategoryResponse
{
    public Guid Id { get; init; }
    public required string Name { get; init; }
}