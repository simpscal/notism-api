using MediatR;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using Notism.Application.Common.Persistence;
using Notism.Application.Common.Services;
using Notism.Domain.Food.Repositories;
using Notism.Shared.Exceptions;

using DomainCategory = Notism.Domain.Food.Category;

namespace Notism.Application.Food.AdminUpdateCategory;

public class AdminUpdateCategoryHandler : IRequestHandler<AdminUpdateCategoryRequest, AdminUpdateCategoryResponse>
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly IReadDbContext _readDbContext;
    private readonly ILogger<AdminUpdateCategoryHandler> _logger;
    private readonly IMessages _messages;

    public AdminUpdateCategoryHandler(
        ICategoryRepository categoryRepository,
        IReadDbContext readDbContext,
        ILogger<AdminUpdateCategoryHandler> logger,
        IMessages messages)
    {
        _categoryRepository = categoryRepository;
        _readDbContext = readDbContext;
        _logger = logger;
        _messages = messages;
    }

    public async Task<AdminUpdateCategoryResponse> Handle(
        AdminUpdateCategoryRequest request,
        CancellationToken cancellationToken)
    {
        var category = await _readDbContext.Set<DomainCategory>(tracking: true)
                .Where(c => c.Id == request.CategoryId && !c.IsDeleted)
                .FirstOrDefaultAsync(cancellationToken)
            ?? throw new NotFoundException(_messages.CategoryNotFound);

        var nameTrimmed = request.Name.Trim();
        var duplicate = await _readDbContext.Set<DomainCategory>()
            .Where(c => !c.IsDeleted && c.Id != request.CategoryId && c.Name.ToLower() == nameTrimmed.ToLower())
            .FirstOrDefaultAsync(cancellationToken);
        if (duplicate != null)
        {
            throw new ResultFailureException(_messages.CategoryAlreadyExists);
        }

        category.UpdateName(request.Name);
        await _categoryRepository.SaveChangesAsync();

        _logger.LogInformation("Updated category {CategoryId} to name {Name}", category.Id, category.Name);

        return AdminUpdateCategoryResponse.FromDomain(category);
    }
}