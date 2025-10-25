using System.Security.Cryptography;

using MediatR;

using Notism.Application.Common.Interfaces;
using Notism.Domain.Common.Interfaces;
using Notism.Domain.User;
using Notism.Domain.User.Specifications;
using Notism.Shared.Models;

namespace Notism.Application.Auth.RequestPasswordReset;

public class RequestPasswordResetUseCase : IRequestHandler<RequestPasswordResetRequest, Result<RequestPasswordResetResponse>>
{
    private readonly IRepository<User> _userRepository;
    private readonly IRepository<PasswordResetToken> _passwordResetTokenRepository;
    private readonly IEmailService _emailService;

    public RequestPasswordResetUseCase(
        IRepository<User> userRepository,
        IRepository<PasswordResetToken> passwordResetTokenRepository,
        IEmailService emailService)
    {
        _userRepository = userRepository;
        _passwordResetTokenRepository = passwordResetTokenRepository;
        _emailService = emailService;
    }

    public async Task<Result<RequestPasswordResetResponse>> Handle(
        RequestPasswordResetRequest request,
        CancellationToken cancellationToken)
    {
        var userByEmailSpec = new UserByEmailSpecification(request.Email);
        var user = await _userRepository.FindByExpressionAsync(userByEmailSpec);

        if (user is null)
        {
            // Don't reveal if email exists for security reasons
            return Result<RequestPasswordResetResponse>.Success(new RequestPasswordResetResponse
            {
                Message = "If the email exists, a password reset link has been sent.",
            });
        }

        // Check if there's already an active token for this user
        var activeTokenSpec = new ActivePasswordResetTokenByUserIdSpecification(user.Id);
        var existingToken = await _passwordResetTokenRepository.FindByExpressionAsync(activeTokenSpec);

        if (existingToken is not null && existingToken.IsValid())
        {
            existingToken.MarkAsUsed();
        }

        var resetToken = GenerateSecureToken();
        var expiresAt = DateTime.UtcNow.AddHours(24);

        var passwordResetToken = PasswordResetToken.Create(resetToken, user.Id, expiresAt);
        await _passwordResetTokenRepository.AddAsync(passwordResetToken);

        // Add domain event to user
        user.RequestPasswordReset(resetToken, expiresAt);

        await _passwordResetTokenRepository.SaveChangesAsync();
        await _emailService.SendPasswordResetEmailAsync(user.Email, resetToken);

        return Result<RequestPasswordResetResponse>.Success(new RequestPasswordResetResponse
        {
            Message = "If the email exists, a password reset link has been sent.",
        });
    }

    private static string GenerateSecureToken()
    {
        const int tokenLength = 32;
        var randomBytes = new byte[tokenLength];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes).Replace("+", "-").Replace("/", "_").Replace("=", string.Empty);
    }
}