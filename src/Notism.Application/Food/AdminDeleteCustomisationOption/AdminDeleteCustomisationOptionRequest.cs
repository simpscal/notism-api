using MediatR;

namespace Notism.Application.Food.AdminDeleteCustomisationOption;

public record AdminDeleteCustomisationOptionRequest : IRequest
{
    public Guid FoodId { get; init; }
    public Guid GroupId { get; init; }
    public Guid OptionId { get; init; }
}