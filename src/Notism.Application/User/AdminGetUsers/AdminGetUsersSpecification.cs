using System.Linq.Expressions;

using Notism.Domain.Common.Specifications;
using Notism.Shared.Enums;
using Notism.Shared.Extensions;

using DomainUser = Notism.Domain.User.User;

namespace Notism.Application.User.AdminGetUsers;

public class AdminGetUsersSpecification : Specification<DomainUser>
{
    private readonly string? _keyword;
    private readonly string? _sortBy;
    private readonly bool _isDescending;

    public AdminGetUsersSpecification(
        string? keyword = null,
        string? sortBy = null,
        string? sortOrder = null)
    {
        _keyword = keyword;
        _sortBy = sortBy;
        var sortOrderEnum = sortOrder?.FromCamelCase<SortOrder>() ?? SortOrder.Asc;
        _isDescending = sortOrderEnum == SortOrder.Desc;
    }

    public override Expression<Func<DomainUser, bool>> ToExpression()
    {
        if (string.IsNullOrWhiteSpace(_keyword))
        {
            return user => true;
        }

        var keywordLower = _keyword!.ToLower();

        return user =>
            (user.FirstName != null && user.FirstName.ToLower().Contains(keywordLower)) ||
            (user.LastName != null && user.LastName.ToLower().Contains(keywordLower)) ||
            ((string)user.Email).ToLower().Contains(keywordLower) ||
            ((string)(object)user.Role).ToLower().Contains(keywordLower);
    }

    public override IQueryable<DomainUser> ApplyOrdering(IQueryable<DomainUser> queryable)
    {
        return _sortBy switch
        {
            "firstName" => _isDescending
                ? queryable.OrderByDescending(u => u.FirstName)
                : queryable.OrderBy(u => u.FirstName),
            "lastName" => _isDescending
                ? queryable.OrderByDescending(u => u.LastName)
                : queryable.OrderBy(u => u.LastName),
            "email" => _isDescending
                ? queryable.OrderByDescending(u => u.Email.Value)
                : queryable.OrderBy(u => u.Email.Value),
            "role" => _isDescending
                ? queryable.OrderByDescending(u => u.Role)
                : queryable.OrderBy(u => u.Role),
            _ => queryable.OrderByDescending(u => u.CreatedAt),
        };
    }
}