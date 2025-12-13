using Notism.Domain.Common;

namespace Notism.Domain.User;

public class PasswordResetToken : Entity
{
    public string Token { get; private set; } = string.Empty;
    public Guid UserId { get; private set; }
    public DateTime ExpiresAt { get; private set; }
    public bool IsUsed { get; private set; }

    public static PasswordResetToken Create(string token, Guid userId, DateTime expiresAt)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            throw new ArgumentException("Token cannot be null or empty", nameof(token));
        }

        if (userId == Guid.Empty)
        {
            throw new ArgumentException("UserId cannot be empty", nameof(userId));
        }

        if (expiresAt <= DateTime.UtcNow)
        {
            throw new ArgumentException("ExpiresAt must be in the future", nameof(expiresAt));
        }

        return new PasswordResetToken
        {
            Id = Guid.NewGuid(),
            Token = token,
            UserId = userId,
            ExpiresAt = expiresAt,
            IsUsed = false,
        };
    }

    public void MarkAsUsed()
    {
        if (IsUsed)
        {
            throw new InvalidOperationException("Token has already been used");
        }

        if (IsExpired())
        {
            throw new InvalidOperationException("Cannot use an expired token");
        }

        IsUsed = true;
    }

    public bool IsExpired()
    {
        return DateTime.UtcNow >= ExpiresAt;
    }

    public bool IsValid()
    {
        return !IsUsed && !IsExpired();
    }
}