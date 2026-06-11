using MediatR;

using Notism.Application.Common.Services;
using Notism.Domain.Common.Persistence;
using Notism.Domain.Common.Repositories;
using Notism.Domain.User;
using Notism.Shared.Exceptions;

namespace Notism.Application.Auth.ResetPassword;

public class ResetPasswordHandler : IRequestHandler<ResetPasswordRequest, ResetPasswordResponse>
{
    private readonly IRepository<Domain.User.User> _userRepository;
    private readonly IPasswordService _passwordService;
    private readonly IRepository<PasswordResetToken> _passwordResetTokenRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMessages _messages;

    public ResetPasswordHandler(
        IRepository<Domain.User.User> userRepository,
        IPasswordService passwordService,
        IRepository<PasswordResetToken> passwordResetTokenRepository,
        IUnitOfWork unitOfWork,
        IMessages messages)
    {
        _userRepository = userRepository;
        _passwordService = passwordService;
        _passwordResetTokenRepository = passwordResetTokenRepository;
        _unitOfWork = unitOfWork;
        _messages = messages;
    }

    public async Task<ResetPasswordResponse> Handle(
        ResetPasswordRequest request,
        CancellationToken cancellationToken)
    {
        var resetToken = await _passwordResetTokenRepository.GetForUpdateAsync(
                t => t.Token == request.Token && !t.IsUsed && t.ExpiresAt > DateTime.UtcNow)
            ?? throw new ResultFailureException(_messages.InvalidOrExpiredResetToken);

        if (!resetToken.IsValid())
        {
            throw new ResultFailureException(_messages.InvalidOrExpiredResetToken);
        }

        await _unitOfWork.ExecuteInTransactionAsync(async () =>
        {
            var user = await _userRepository.GetForUpdateAsync(u => u.Id == resetToken.UserId)
                ?? throw new ResultFailureException(_messages.UserNotFound);

            var hashedPassword = _passwordService.HashPassword(request.NewPassword);
            user.ResetPassword(hashedPassword);

            resetToken.MarkAsUsed();
        });

        return new ResetPasswordResponse
        {
            Message = _messages.PasswordResetSuccess,
        };
    }
}