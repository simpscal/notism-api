using Notism.Domain.Common;
using Notism.Domain.User.Events;
using Notism.Domain.User.ValueObjects;

namespace Notism.Domain.User;

public class User : AggregateRoot
{
    public Email Email { get; private set; }
    public Password Password { get; private set; }

    private User(Email email, Password password)
    {
        Email = email;
        Password = password;
        AddDomainEvent(new UserCreatedEvent(Id, Email));
    }

    public static User Create(string email, string password)
    {
        var emailVO = Email.Create(email);
        var passwordVO = Password.Create(password);

        return new User(emailVO, passwordVO);
    }

    public User WithHashedPassword(string hashedPassword)
    {
        var copy = (User)MemberwiseClone();

        copy.Password = Password.Create(hashedPassword);
        copy.ClearDomainEvents();

        return copy;
    }
}