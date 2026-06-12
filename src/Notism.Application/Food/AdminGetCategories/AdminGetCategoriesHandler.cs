using MediatR;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using Notism.Application.Common.Persistence;
using Notism.Application.Food.Common;

using DomainCategory = Notism.Domain.Food.Category;

namespace Notism.Application.Food.AdminGetCategories;

public class AdminGetCategoriesHandler : IRequestHandler<AdminGetCategoriesRequest, AdminGetCategoriesResponse>
{
    private readonly IReadDbContext _readDbContext;
    private readonly ILogger<AdminGetCategoriesHandler> _logger;

    public AdminGetCategoriesHandler(
        IReadDbContext readDbContext,
        ILogger<AdminGetCategoriesHandler> logger)
    {
        _readDbContext = readDbContext;
        _logger = logger;
    }

    public async Task<AdminGetCategoriesResponse> Handle(
        AdminGetCategoriesRequest request,
        CancellationToken cancellationToken)
    {
        var categories = await _readDbContext.Set<DomainCategory>()
            .Where(c => !c.IsDeleted)
            .OrderBy(c => c.Name)
            .ToListAsync(cancellationToken);
        var items = categories
            .Select(CategoryResponse.FromDomain)
            .ToList();

        _logger.LogInformation("Retrieved {Count} categories for admin", items.Count);

        return new AdminGetCategoriesResponse
        {
            Items = items,
            TotalCount = items.Count,
        };
    }
}