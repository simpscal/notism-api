using Order.Domain.Common;

namespace Order.Domain.User;

public class User : Entity
{
    public required string Email { get; set; }
    public required string Password { get; set; }
}