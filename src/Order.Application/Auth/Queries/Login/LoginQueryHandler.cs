using MediatR;

using Microsoft.AspNetCore.Identity;

using Order.Application.Common.Models;
using Order.Application.Common.Utilities;
using Order.Domain.User;
using Order.Domain.User.Specifications;

namespace Order.Application.Auth.Queries.Login;

public class LoginQueryHandler(
    IUserRepository userRepository,
    TokenUtility tokenUtility)
    : IRequestHandler<LoginQuery, JwtToken>
{
    public async Task<JwtToken> Handle(LoginQuery request, CancellationToken cancellationToken)
    {
        var existingUserSpecification = new ExistingUserSpecification(request.Email);
        var user = await userRepository.FindByExpressionAsync(existingUserSpecification);

        if (user == null || !IsPasswordValid(user.Password, request.Password))
        {
            throw new Exception("Invalid email or password");
        }

        return tokenUtility.GenerateJwtToken(user);
    }

    private bool IsPasswordValid(string dbPassword, string requestPassword)
    {
        var passwordHasher = new PasswordHasher<object>();
        var verificationResult = passwordHasher.VerifyHashedPassword(new object(), dbPassword, requestPassword);

        return verificationResult == PasswordVerificationResult.Success;
    }
}

public record LoginQuery(string Email, string Password) : IRequest<JwtToken>;