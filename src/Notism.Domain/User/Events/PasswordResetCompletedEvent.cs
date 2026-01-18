using Notism.Domain.Common;
using Notism.Domain.User.ValueObjects;

namespace Notism.Domain.User.Events;

public class PasswordResetCompletedEvent : DomainEvent
{
    public Guid UserId { get; }
    public Email Email { get; }

    public PasswordResetCompletedEvent(Guid userId, Email email)
    {
        UserId = userId;
        Email = email;
    }
}