using Notism.Domain.Common;

namespace Notism.Domain.User;

public class User : Entity
{
    public required string Email { get; set; }
    public required string Password { get; set; }
}