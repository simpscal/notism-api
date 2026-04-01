using MediatR;

using Notism.Application.Common.Interfaces;
using Notism.Application.Common.Services;
using Notism.Domain.Common.Interfaces;
using Notism.Domain.Common.Specifications;
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
        var tokenSpec = new FilterSpecification<PasswordResetToken>(t => t.Token == request.Token && !t.IsUsed && t.ExpiresAt > DateTime.UtcNow);
        var resetToken = await _passwordResetTokenRepository.FindByExpressionAsync(tokenSpec)
            ?? throw new ResultFailureException(_messages.InvalidOrExpiredResetToken);

        if (!resetToken.IsValid())
        {
            throw new ResultFailureException(_messages.InvalidOrExpiredResetToken);
        }

        await _unitOfWork.ExecuteInTransactionAsync(async () =>
        {
            var userSpec = new FilterSpecification<Domain.User.User>(u => u.Id == resetToken.UserId);
            var user = await _userRepository.FindByExpressionAsync(userSpec) ?? throw new ResultFailureException(_messages.UserNotFound);

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