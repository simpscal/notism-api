using Notism.Domain.Common;
using Notism.Domain.User.Enums;
using Notism.Domain.User.ValueObjects;

namespace Notism.Domain.User.Events;

public class UserProfileUpdatedEvent : DomainEvent
{
    public Guid UserId { get; }
    public Email Email { get; }
    public string? FirstName { get; }
    public string? LastName { get; }
    public UserRole Role { get; }
    public string? AvatarUrl { get; }

    public UserProfileUpdatedEvent(Guid userId, Email email, string? firstName, string? lastName, UserRole role, string? avatarUrl)
    {
        UserId = userId;
        Email = email;
        FirstName = firstName;
        LastName = lastName;
        Role = role;
        AvatarUrl = avatarUrl;
    }
}