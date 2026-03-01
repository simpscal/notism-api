using MediatR;

namespace Notism.Application.Food.AdminGetCategories;

public record AdminGetCategoriesRequest : IRequest<AdminGetCategoriesResponse>;
