using Notism.Domain.Common;

namespace Notism.Domain.User.ValueObjects;

public class Password : ValueObject
{
    public string Value { get; private set; }

    private Password(string value)
    {
        Value = value;
    }

    public static Password Create(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
        {
            throw new ArgumentException("Password cannot be empty", nameof(password));
        }

        if (password.Length < 8)
        {
            throw new ArgumentException("Password must be at least 8 characters long", nameof(password));
        }

        return new Password(password);
    }

    public static implicit operator string(Password password) => password.Value;

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}