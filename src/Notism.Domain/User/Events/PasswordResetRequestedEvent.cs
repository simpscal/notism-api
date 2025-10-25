using Notism.Domain.Common;
using Notism.Domain.User.ValueObjects;

namespace Notism.Domain.User.Events;

public class PasswordResetRequestedEvent : DomainEvent
{
    public Guid UserId { get; }
    public Email Email { get; }
    public string ResetToken { get; }
    public DateTime ExpiresAt { get; }

    public PasswordResetRequestedEvent(Guid userId, Email email, string resetToken, DateTime expiresAt)
    {
        UserId = userId;
        Email = email;
        ResetToken = resetToken;
        ExpiresAt = expiresAt;
    }
}