using System.Security.Cryptography;

using MediatR;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using Notism.Application.Common.Persistence;
using Notism.Application.Common.Services;
using Notism.Domain.Common.Persistence;
using Notism.Domain.Common.Repositories;
using Notism.Domain.User;
using Notism.Domain.User.ValueObjects;
using Notism.Shared.Exceptions;

using DomainUser = Notism.Domain.User.User;

namespace Notism.Application.Auth.RequestPasswordReset;

public class RequestPasswordResetHandler : IRequestHandler<RequestPasswordResetRequest, RequestPasswordResetResponse>
{
    private readonly IReadDbContext _readDbContext;
    private readonly IRepository<PasswordResetToken> _passwordResetTokenRepository;
    private readonly IEmailService _emailService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<RequestPasswordResetHandler> _logger;
    private readonly IMessages _messages;

    public RequestPasswordResetHandler(
        IReadDbContext readDbContext,
        IRepository<PasswordResetToken> passwordResetTokenRepository,
        IEmailService emailService,
        IUnitOfWork unitOfWork,
        ILogger<RequestPasswordResetHandler> logger,
        IMessages messages)
    {
        _readDbContext = readDbContext;
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
        var user = await _readDbContext.Set<DomainUser>()
            .Where(u => u.Email.Equals(email))
            .FirstOrDefaultAsync(cancellationToken);

        if (user is null)
        {
            // Don't reveal if email exists for security reasons
            return new RequestPasswordResetResponse
            {
                Message = _messages.PasswordResetEmailSent,
            };
        }

        var existingToken = await _readDbContext.Set<PasswordResetToken>(tracking: true)
            .Where(t => t.UserId == user.Id && !t.IsUsed && t.ExpiresAt > DateTime.UtcNow)
            .FirstOrDefaultAsync(cancellationToken);

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