using System.Security.Cryptography;

using MediatR;

using Microsoft.Extensions.Logging;

using Notism.Application.Common.Interfaces;
using Notism.Application.Common.Services;
using Notism.Domain.Common.Interfaces;
using Notism.Domain.Common.Specifications;
using Notism.Domain.User;
using Notism.Domain.User.ValueObjects;
using Notism.Shared.Exceptions;

namespace Notism.Application.Auth.RequestPasswordReset;

public class RequestPasswordResetHandler : IRequestHandler<RequestPasswordResetRequest, RequestPasswordResetResponse>
{
    private readonly IRepository<Domain.User.User> _userRepository;
    private readonly IRepository<PasswordResetToken> _passwordResetTokenRepository;
    private readonly IEmailService _emailService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<RequestPasswordResetHandler> _logger;
    private readonly IMessages _messages;

    public RequestPasswordResetHandler(
        IRepository<Domain.User.User> userRepository,
        IRepository<PasswordResetToken> passwordResetTokenRepository,
        IEmailService emailService,
        IUnitOfWork unitOfWork,
        ILogger<RequestPasswordResetHandler> logger,
        IMessages messages)
    {
        _userRepository = userRepository;
        _passwordResetTokenRepository = passwordResetTokenRepository;
        _emailService = emailService;
        _unitOfWork = unitOfWork;
        _logger = logger;
        _messages = messages;
    }

    public async Task<RequestPasswordResetResponse> Handle(
        RequestPasswordResetRequest request,
        CancellationToken cancellationToken)
    {
        var email = Email.Create(request.Email);
        var userSpec = new FilterSpecification<Domain.User.User>(u => u.Email.Equals(email));
        var user = await _userRepository.FindByExpressionAsync(userSpec);

        if (user is null)
        {
            // Don't reveal if email exists for security reasons
            return new RequestPasswordResetResponse
            {
                Message = _messages.PasswordResetEmailSent,
            };
        }

        // Check if there's already an active token for this user
        var tokenSpec = new FilterSpecification<PasswordResetToken>(t => t.UserId == user.Id && !t.IsUsed && t.ExpiresAt > DateTime.UtcNow);
        var existingToken = await _passwordResetTokenRepository.FindByExpressionAsync(tokenSpec);

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
                throw new ResultFailureException(_messages.RequestPasswordResetFailed);
            }

            _logger.LogInformation("Password reset token created for user {UserId}. Attempting to send email.", user.Id);

            await _emailService.SendPasswordResetEmailAsync(user.Email, resetToken);

            _logger.LogInformation("Password reset email sent successfully for user {UserId}", user.Id);

            return new RequestPasswordResetResponse
            {
                Message = _messages.PasswordResetEmailSent,
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to complete password reset process for user {UserId}", user.Id);
            throw new ResultFailureException(_messages.RequestPasswordResetFailed);
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