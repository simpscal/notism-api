using MediatR;

using Notism.Application.Common.Persistence;
using Notism.Application.Common.Services;
using Notism.Shared.Exceptions;

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
        var category = await new AdminGetCategoryDetailQuery(_readDbContext).ExecuteAsync(request.CategoryId, cancellationToken)
            ?? throw new NotFoundException(_messages.CategoryNotFound);

        return AdminGetCategoryDetailResponse.FromDomain(category);
    }
}