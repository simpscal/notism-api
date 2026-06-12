using MediatR;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using Notism.Application.Common.Persistence;
using Notism.Application.Common.Services;
using Notism.Domain.Food;
using Notism.Domain.Food.Repositories;
using Notism.Shared.Exceptions;

using DomainCategory = Notism.Domain.Food.Category;

namespace Notism.Application.Food.AdminAddCategory;

public class AdminAddCategoryHandler : IRequestHandler<AdminAddCategoryRequest, AdminAddCategoryResponse>
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly IReadDbContext _readDbContext;
    private readonly ILogger<AdminAddCategoryHandler> _logger;
    private readonly IMessages _messages;

    public AdminAddCategoryHandler(
        ICategoryRepository categoryRepository,
        IReadDbContext readDbContext,
        ILogger<AdminAddCategoryHandler> logger,
        IMessages messages)
    {
        _categoryRepository = categoryRepository;
        _readDbContext = readDbContext;
        _logger = logger;
        _messages = messages;
    }

    public async Task<AdminAddCategoryResponse> Handle(
        AdminAddCategoryRequest request,
        CancellationToken cancellationToken)
    {
        var nameTrimmed = request.Name.Trim();
        var existing = await _readDbContext.Set<DomainCategory>()
            .Where(c => !c.IsDeleted && c.Name.ToLower() == nameTrimmed.ToLower())
            .FirstOrDefaultAsync(cancellationToken);
        if (existing != null)
        {
            throw new ResultFailureException(_messages.CategoryAlreadyExists);
        }

        var category = Notism.Domain.Food.Category.Create(request.Name);

        _categoryRepository.Add(category);
        await _categoryRepository.SaveChangesAsync();

        _logger.LogInformation("Added category {CategoryId} with name {Name}", category.Id, category.Name);

        return AdminAddCategoryResponse.FromDomain(category);
    }
}