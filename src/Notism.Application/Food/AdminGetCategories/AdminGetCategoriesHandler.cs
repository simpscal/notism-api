using MediatR;

using Microsoft.Extensions.Logging;

using Notism.Application.Common.Persistence;
using Notism.Application.Food.Common;

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
        var categories = await new AdminGetCategoriesQuery(_readDbContext).ExecuteAsync(cancellationToken);
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