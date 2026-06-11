using Notism.Application.Common.Persistence;
using Notism.Domain.User.ValueObjects;

using DomainUser = Notism.Domain.User.User;

namespace Notism.Application.Auth.Register;

/// <summary>
/// Self-contained read: tests whether a user already exists for the given email.
/// Owned by <see cref="RegisterHandler"/>; the by-email predicate is duplicated inline
/// here rather than shared with any other handler.
/// </summary>
public sealed class UserExistsByEmailQuery
{
    private readonly IReadDbContext _readDbContext;

    public UserExistsByEmailQuery(IReadDbContext readDbContext)
    {
        _readDbContext = readDbContext;
    }

    public Task<bool> ExecuteAsync(Email email, CancellationToken cancellationToken = default)
    {
        var query = _readDbContext.Set<DomainUser>()
            .Where(u => u.Email.Equals(email));

        return _readDbContext.AnyAsync(query, cancellationToken);
    }
}
