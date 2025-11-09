using System.Security.Cryptography;

using MediatR;

using Microsoft.Extensions.Logging;

using Notism.Application.Common.Interfaces;
using Notism.Domain.Common.Interfaces;
using Notism.Domain.User;
using Notism.Domain.User.Specifications;
using Notism.Shared.Exceptions;

namespace Notism.Application.Auth.RequestPasswordReset;

public class RequestPasswordResetHandler : IRequestHandler<RequestPasswordResetRequest, RequestPasswordResetResponse>
{
    private readonly IRepository<Domain.User.User> _userRepository;
    private readonly IRepository<PasswordResetToken> _passwordResetTokenRepository;
    private readonly IEmailService _emailService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<RequestPasswordResetHandler> _logger;

    public RequestPasswordResetHandler(
        IRepository<Domain.User.User> userRepository,
        IRepository<PasswordResetToken> passwordResetTokenRepository,
        IEmailService emailService,
        IUnitOfWork unitOfWork,
        ILogger<RequestPasswordResetHandler> logger)
    {
        _userRepository = userRepository;
        _passwordResetTokenRepository = passwordResetTokenRepository;
        _emailService = emailService;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<RequestPasswordResetResponse> Handle(
        RequestPasswordResetRequest request,
        CancellationToken cancellationToken)
    {
        var user = await _userRepository.FindByExpressionAsync(new UserByEmailSpecification(request.Email));

        if (user is null)
        {
            // Don't reveal if email exists for security reasons
            return new RequestPasswordResetResponse
            {
                Message = "If the email exists, a password reset link has been sent.",
            };
        }

        // Check if there's already an active token for this user
        var existingToken = await _passwordResetTokenRepository.FindByExpressionAsync(
            new ActivePasswordResetTokenByUserIdSpecification(user.Id));

        if (existingToken is not null && existingToken.IsValid())
        {
            existingToken.MarkAsUsed();
        }

        var resetToken = GenerateSecureToken();
        var expiresAt = DateTime.UtcNow.AddHours(24);

        try
        {
            var passwordResetToken = PasswordResetToken.Create(resetToken, user.Id, expiresAt);
            await _passwordResetTokenRepository.AddAsync(passwordResetToken);

            // Add domain event to user
            user.RequestPasswordReset(resetToken, expiresAt);

            var result = await _passwordResetTokenRepository.SaveChangesAsync();
            if (result < 1)
            {
                throw new ResultFailureException("Failed to process password reset request. Please try again later.");
            }

            _logger.LogInformation("Password reset token created for user {UserId}. Attempting to send email.", user.Id);

            await _emailService.SendPasswordResetEmailAsync(user.Email, resetToken);

            _logger.LogInformation("Password reset email sent successfully for user {UserId}", user.Id);

            return new RequestPasswordResetResponse
            {
                Message = "If the email exists, a password reset link has been sent.",
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to complete password reset process for user {UserId}", user.Id);
            throw new ResultFailureException("Failed to process password reset request. Please try again later.");
        }
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