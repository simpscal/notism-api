using Notism.Application.Common.Persistence;

using DomainUser = Notism.Domain.User.User;

namespace Notism.Application.User.GetProfile;

/// <summary>
/// Self-contained read: resolves the requesting user's own profile by id. Owned by
/// <see cref="GetUserProfileHandler"/>; the by-id predicate is duplicated inline here
/// rather than shared with any other handler.
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
