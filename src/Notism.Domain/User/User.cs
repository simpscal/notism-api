using Notism.Domain.Common;
using Notism.Domain.User.Enums;
using Notism.Domain.User.Events;
using Notism.Domain.User.ValueObjects;

namespace Notism.Domain.User;

public class User : AggregateRoot
{
    public Email Email { get; private set; }
    public Password Password { get; private set; }
    public UserRole Role { get; private set; }
    public string? FirstName { get; private set; }
    public string? LastName { get; private set; }
    public string? AvatarUrl { get; private set; }

    private User(Email email, Password password, UserRole role = UserRole.User, string? firstName = null, string? lastName = null, string? avatarUrl = null)
    {
        Email = email;
        Password = password;
        Role = role;
        FirstName = firstName;
        LastName = lastName;
        AvatarUrl = avatarUrl;
        AddDomainEvent(new UserCreatedEvent(Id, Email));
    }

    public static User Create(string email, string password, UserRole role = UserRole.User, string? firstName = null, string? lastName = null, string? avatarUrl = null)
    {
        var emailVO = Email.Create(email);
        var passwordVO = Password.Create(password);

        return new User(emailVO, passwordVO, role, firstName, lastName, avatarUrl);
    }

    public User WithHashedPassword(string hashedPassword)
    {
        var copy = (User)MemberwiseClone();

        copy.Password = Password.Create(hashedPassword);
        copy.ClearDomainEvents();

        return copy;
    }

    public void RequestPasswordReset(string resetToken, DateTime expiresAt)
    {
        AddDomainEvent(new PasswordResetRequestedEvent(Id, Email, resetToken, expiresAt));
    }

    public void ResetPassword(string newHashedPassword)
    {
        Password = Password.Create(newHashedPassword);
        AddDomainEvent(new PasswordResetCompletedEvent(Id, Email));
    }

    public void UpdateProfile(string? firstName, string? lastName, string? avatarUrl = null)
    {
        FirstName = firstName;
        LastName = lastName;
        AvatarUrl = avatarUrl;

        ClearDomainEvents();
        AddDomainEvent(new UserProfileUpdatedEvent(Id, Email, FirstName, LastName, Role, AvatarUrl));
    }

    public bool IsAdmin() => Role == UserRole.Admin;
}