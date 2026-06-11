using MediatR;

using Microsoft.Extensions.Logging;

using Notism.Application.Common.Persistence;

namespace Notism.Application.User.AdminGetUsers;

public class AdminGetUsersHandler : IRequestHandler<AdminGetUsersRequest, AdminGetUsersResponse>
{
    private readonly IReadDbContext _readDbContext;
    private readonly ILogger<AdminGetUsersHandler> _logger;

    public AdminGetUsersHandler(
        IReadDbContext readDbContext,
        ILogger<AdminGetUsersHandler> logger)
    {
        _readDbContext = readDbContext;
        _logger = logger;
    }

    public async Task<AdminGetUsersResponse> Handle(
        AdminGetUsersRequest request,
        CancellationToken cancellationToken)
    {
        var (totalCount, users) = await new AdminGetUsersQuery(_readDbContext).ExecuteAsync(
            request.Keyword,
            request.SortBy,
            request.SortOrder,
            request.Skip,
            request.Take,
            cancellationToken);

        var items = users.Select(AdminGetUsersItemResponse.FromDomain).ToList();

        _logger.LogInformation("Retrieved {Count} users for admin portal", items.Count);

        return new AdminGetUsersResponse
        {
            TotalCount = totalCount,
            Items = items,
        };
    }
}