using MediatR;

namespace Notism.Application.Food.GetCategories;

public record GetCategoriesRequest : IRequest<GetCategoriesResponse>;