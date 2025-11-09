using MediatR;

using Notism.Application.Common.Interfaces;
using Notism.Domain.Common.Interfaces;
using Notism.Domain.User;
using Notism.Domain.User.Specifications;
using Notism.Shared.Exceptions;
using Notism.Shared.Models;

namespace Notism.Application.Auth.ResetPassword;

public class ResetPasswordHandler : IRequestHandler<ResetPasswordRequest, Result<ResetPasswordResponse>>
{
    private readonly IRepository<Domain.User.User> _userRepository;
    private readonly IPasswordService _passwordService;
    private readonly IRepository<PasswordResetToken> _passwordResetTokenRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ResetPasswordHandler(
        IRepository<Domain.User.User> userRepository,
        IPasswordService passwordService,
        IRepository<PasswordResetToken> passwordResetTokenRepository,
        IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _passwordService = passwordService;
        _passwordResetTokenRepository = passwordResetTokenRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<ResetPasswordResponse>> Handle(
        ResetPasswordRequest request,
        CancellationToken cancellationToken)
    {
        var resetToken = await _passwordResetTokenRepository.FindByExpressionAsync(
            new PasswordResetTokenByTokenSpecification(request.Token))
        ?? throw new ResultFailureException("Invalid or expired reset token");

        if (!resetToken.IsValid())
        {
            throw new ResultFailureException("Invalid or expired reset token");
        }

        await _unitOfWork.ExecuteInTransactionAsync(async () =>
        {
            var user = await _userRepository.FindByExpressionAsync(new UserByIdSpecification(resetToken.UserId)) ?? throw new ResultFailureException("User not found");

            var hashedPassword = _passwordService.HashPassword(request.NewPassword);
            user.ResetPassword(hashedPassword);

            resetToken.MarkAsUsed();
        });

        return Result<ResetPasswordResponse>.Success(new ResetPasswordResponse
        {
            Message = "Password has been successfully reset.",
        });
    }
}