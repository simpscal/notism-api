using Notism.Application.Common.Persistence;
using Notism.Domain.User.ValueObjects;

using DomainUser = Notism.Domain.User.User;

namespace Notism.Application.Auth.Login;

/// <summary>
/// Self-contained read: resolves the user with the given email for credential
/// verification. Owned by <see cref="LoginHandler"/>; the by-email predicate is
/// duplicated inline here rather than shared with any other handler.
/// </summary>
public sealed class GetUserByEmailQuery
{
    private readonly IReadDbContext _readDbContext;

    public GetUserByEmailQuery(IReadDbContext readDbContext)
    {
        _readDbContext = readDbContext;
    }

    public Task<DomainUser?> ExecuteAsync(Email email, CancellationToken cancellationToken = default)
    {
        var query = _readDbContext.Set<DomainUser>()
            .Where(u => u.Email.Equals(email));

        return _readDbContext.FirstOrDefaultAsync(query, cancellationToken);
    }
}
