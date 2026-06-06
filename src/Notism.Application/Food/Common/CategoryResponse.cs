using Notism.Domain.Food;

namespace Notism.Application.Food.Common;

public record CategoryResponse
{
    public Guid Id { get; init; }
    public required string Name { get; init; }

    public static CategoryResponse FromDomain(Category category)
    {
        return new CategoryResponse
        {
            Id = category.Id,
            Name = category.Name,
        };
    }
}