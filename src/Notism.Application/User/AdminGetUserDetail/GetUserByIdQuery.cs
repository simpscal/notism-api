using Notism.Application.Common.Persistence;

using DomainUser = Notism.Domain.User.User;

namespace Notism.Application.User.AdminGetUserDetail;

/// <summary>
/// Self-contained read: resolves a single user by id for the admin detail view. Owned
/// by <see cref="AdminGetUserDetailHandler"/>; the by-id predicate is duplicated inline
/// here rather than shared with any other handler.
/// </summary>
public sealed class GetUserByIdQuery
{
    private readonly IReadDbContext _readDbContext;

    public GetUserByIdQuery(IReadDbContext readDbContext)
    {
        _readDbContext = readDbContext;
    }

    public Task<DomainUser?> ExecuteAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var query = _readDbContext.Set<DomainUser>()
            .Where(u => u.Id == userId);

        return _readDbContext.FirstOrDefaultAsync(query, cancellationToken);
    }
}
