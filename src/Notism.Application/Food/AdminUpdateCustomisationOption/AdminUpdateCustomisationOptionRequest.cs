using MediatR;

namespace Notism.Application.Food.AdminUpdateCustomisationOption;

public record AdminUpdateCustomisationOptionRequest : IRequest<AdminUpdateCustomisationOptionResponse>
{
    public Guid FoodId { get; set; }
    public Guid GroupId { get; set; }
    public Guid OptionId { get; set; }
    public string? Label { get; set; }
    public decimal? Surcharge { get; set; }
    public int? DisplayOrder { get; set; }
}