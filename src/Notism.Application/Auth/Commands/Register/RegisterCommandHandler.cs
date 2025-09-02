using MediatR;

using Notism.Application.Common.Models;
using Notism.Application.Common.Utilities;
using Notism.Domain.User;
using Notism.Domain.User.Specifications;

namespace Notism.Application.Auth.Commands.Register;

public class RegisterCommandHandler(
    IUserRepository userRepository,
    TokenUtility tokenUtility)
    : IRequestHandler<RegisterCommand, JwtToken>
{
    public async Task<JwtToken> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        var existingUserSpecification = new ExistingUserSpecification(request.Email);
        var existingUser = await userRepository.FindByExpressionAsync(existingUserSpecification);

        if (existingUser != null)
        {
            throw new Exception("User already exists");
        }

        var user = await userRepository.AddAsync(new User { Email = request.Email, Password = request.Password, });

        if (await userRepository.SaveChangesAsync() < 1)
        {
            throw new ApplicationException("There was an error creating the user");
        }

        return tokenUtility.GenerateJwtToken(user);
    }
}

public record RegisterCommand(string Email, string Password) : IRequest<JwtToken>;