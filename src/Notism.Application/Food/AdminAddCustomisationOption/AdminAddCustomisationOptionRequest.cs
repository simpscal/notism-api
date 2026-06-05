using MediatR;

namespace Notism.Application.Food.AdminAddCustomisationOption;

public record AdminAddCustomisationOptionRequest : IRequest<AdminAddCustomisationOptionResponse>
{
    public Guid FoodId { get; set; }
    public Guid GroupId { get; set; }
    public required string Label { get; set; }
    public decimal? Surcharge { get; set; }
    public int DisplayOrder { get; set; }
}
