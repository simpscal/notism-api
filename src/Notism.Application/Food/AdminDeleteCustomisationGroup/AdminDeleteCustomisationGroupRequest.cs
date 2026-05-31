using MediatR;

namespace Notism.Application.Food.AdminDeleteCustomisationGroup;

public record AdminDeleteCustomisationGroupRequest : IRequest
{
    public Guid FoodId { get; init; }
    public Guid GroupId { get; init; }
}
