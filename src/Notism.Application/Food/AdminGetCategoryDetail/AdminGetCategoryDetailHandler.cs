using MediatR;

using Notism.Application.Common.Services;
using Notism.Domain.Common.Specifications;
using Notism.Domain.Food;
using Notism.Shared.Exceptions;

namespace Notism.Application.Food.AdminGetCategoryDetail;

public class AdminGetCategoryDetailHandler : IRequestHandler<AdminGetCategoryDetailRequest, AdminGetCategoryDetailResponse>
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly IMessages _messages;

    public AdminGetCategoryDetailHandler(
        ICategoryRepository categoryRepository,
        IMessages messages)
    {
        _categoryRepository = categoryRepository;
        _messages = messages;
    }

    public async Task<AdminGetCategoryDetailResponse> Handle(
        AdminGetCategoryDetailRequest request,
        CancellationToken cancellationToken)
    {
        var specification = new FilterSpecification<Notism.Domain.Food.Category>(
            c => c.Id == request.CategoryId && !c.IsDeleted);
        var category = await _categoryRepository.FindByExpressionAsync(specification)
            ?? throw new NotFoundException(_messages.CategoryNotFound);

        return new AdminGetCategoryDetailResponse
        {
            Id = category.Id,
            Name = category.Name,
        };
    }
}