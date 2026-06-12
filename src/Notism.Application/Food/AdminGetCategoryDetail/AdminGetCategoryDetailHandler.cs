using MediatR;

using Microsoft.EntityFrameworkCore;

using Notism.Application.Common.Persistence;
using Notism.Application.Common.Services;
using Notism.Shared.Exceptions;

using DomainCategory = Notism.Domain.Food.Category;

namespace Notism.Application.Food.AdminGetCategoryDetail;

public class AdminGetCategoryDetailHandler : IRequestHandler<AdminGetCategoryDetailRequest, AdminGetCategoryDetailResponse>
{
    private readonly IReadDbContext _readDbContext;
    private readonly IMessages _messages;

    public AdminGetCategoryDetailHandler(
        IReadDbContext readDbContext,
        IMessages messages)
    {
        _readDbContext = readDbContext;
        _messages = messages;
    }

    public async Task<AdminGetCategoryDetailResponse> Handle(
        AdminGetCategoryDetailRequest request,
        CancellationToken cancellationToken)
    {
        var category = await _readDbContext.Set<DomainCategory>()
                .Where(c => c.Id == request.CategoryId && !c.IsDeleted)
                .FirstOrDefaultAsync(cancellationToken)
            ?? throw new NotFoundException(_messages.CategoryNotFound);

        return AdminGetCategoryDetailResponse.FromDomain(category);
    }
}