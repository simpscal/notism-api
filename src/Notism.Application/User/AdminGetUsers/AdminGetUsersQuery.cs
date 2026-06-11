using Notism.Application.Common.Persistence;
using Notism.Shared.Enums;
using Notism.Shared.Extensions;

using DomainUser = Notism.Domain.User.User;

namespace Notism.Application.User.AdminGetUsers;

/// <summary>
/// Self-contained read for the admin user list: keyword filter, sort and page window
/// all execute server-side over the no-tracking user set. Owned by
/// <see cref="AdminGetUsersHandler"/>; every predicate and ordering is duplicated
/// inline here rather than shared with any other handler.
/// </summary>
public sealed class AdminGetUsersQuery
{
    private readonly IReadDbContext _readDbContext;

    public AdminGetUsersQuery(IReadDbContext readDbContext)
    {
        _readDbContext = readDbContext;
    }

    public async Task<(int TotalCount, List<DomainUser> Items)> ExecuteAsync(
        string? keyword,
        string? sortBy,
        string? sortOrder,
        int skip,
        int take,
        CancellationToken cancellationToken = default)
    {
        var isDescending = (sortOrder?.FromCamelCase<SortOrder>() ?? SortOrder.Asc) == SortOrder.Desc;

        var query = _readDbContext.Set<DomainUser>();

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            var keywordLower = keyword.ToLower();

            query = query.Where(user =>
                (user.FirstName != null && user.FirstName.ToLower().Contains(keywordLower)) ||
                (user.LastName != null && user.LastName.ToLower().Contains(keywordLower)) ||
                ((string)user.Email).ToLower().Contains(keywordLower) ||
                ((string)(object)user.Role).ToLower().Contains(keywordLower));
        }

        query = sortBy switch
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

        var totalCount = await _readDbContext.CountAsync(query, cancellationToken);
        var items = await _readDbContext.ToListAsync(query.Skip(skip).Take(take), cancellationToken);

        return (totalCount, items);
    }
}
