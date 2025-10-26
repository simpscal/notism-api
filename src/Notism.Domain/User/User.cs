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

    private User(Email email, Password password, UserRole role = UserRole.User, string? firstName = null, string? lastName = null)
    {
        Email = email;
        Password = password;
        Role = role;
        FirstName = firstName;
        LastName = lastName;
        AddDomainEvent(new UserCreatedEvent(Id, Email));
    }

    public static User Create(string email, string password, UserRole role = UserRole.User, string? firstName = null, string? lastName = null)
    {
        var emailVO = Email.Create(email);
        var passwordVO = Password.Create(password);

        return new User(emailVO, passwordVO, role, firstName, lastName);
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

    public User ResetPassword(string newHashedPassword)
    {
        var copy = (User)MemberwiseClone();
        copy.Password = Password.Create(newHashedPassword);
        copy.ClearDomainEvents();
        copy.AddDomainEvent(new PasswordResetCompletedEvent(Id, Email));

        return copy;
    }

    public User UpdateProfile(string? firstName, string? lastName, string? email = null, UserRole? role = null)
    {
        var copy = (User)MemberwiseClone();
        copy.FirstName = firstName;
        copy.LastName = lastName;

        if (!string.IsNullOrWhiteSpace(email))
        {
            copy.Email = Email.Create(email);
        }

        if (role.HasValue)
        {
            copy.Role = role.Value;
        }

        copy.ClearDomainEvents();
        copy.AddDomainEvent(new UserProfileUpdatedEvent(Id, copy.Email, copy.FirstName, copy.LastName, copy.Role));

        return copy;
    }

    public bool IsAdmin() => Role == UserRole.Admin;
}