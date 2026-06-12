using MediatR;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using Notism.Application.Common.Persistence;
using Notism.Shared.Enums;
using Notism.Shared.Extensions;

using DomainUser = Notism.Domain.User.User;

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
        var isDescending = (request.SortOrder?.FromCamelCase<SortOrder>() ?? SortOrder.Asc) == SortOrder.Desc;

        var query = _readDbContext.Set<DomainUser>();

        if (!string.IsNullOrWhiteSpace(request.Keyword))
        {
            var keywordLower = request.Keyword.ToLower();

            query = query.Where(user =>
                (user.FirstName != null && user.FirstName.ToLower().Contains(keywordLower)) ||
                (user.LastName != null && user.LastName.ToLower().Contains(keywordLower)) ||
                ((string)user.Email).ToLower().Contains(keywordLower) ||
                ((string)(object)user.Role).ToLower().Contains(keywordLower));
        }

        query = request.SortBy switch
        {
            "firstName" => isDescending
                ? query.OrderByDescending(u => u.FirstName)
                : query.OrderBy(u => u.FirstName),
            "lastName" => isDescending
                ? query.OrderByDescending(u => u.LastName)
                : query.OrderBy(u => u.LastName),
            "email" => isDescending
                ? query.OrderByDescending(u => u.Email.Value)
                : query.OrderBy(u => u.Email.Value),
            "role" => isDescending
                ? query.OrderByDescending(u => u.Role)
                : query.OrderBy(u => u.Role),
            _ => query.OrderByDescending(u => u.CreatedAt),
        };

        var totalCount = await query.CountAsync(cancellationToken);
        var users = await query.Skip(request.Skip).Take(request.Take).ToListAsync(cancellationToken);

        var items = users.Select(AdminGetUsersItemResponse.FromDomain).ToList();

        _logger.LogInformation("Retrieved {Count} users for admin portal", items.Count);

        return new AdminGetUsersResponse
        {
            TotalCount = totalCount,
            Items = items,
        };
    }
}