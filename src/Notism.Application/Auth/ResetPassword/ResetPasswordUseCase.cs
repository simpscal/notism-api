using MediatR;

using Notism.Application.Common.Interfaces;
using Notism.Domain.Common.Interfaces;
using Notism.Domain.User;
using Notism.Domain.User.Specifications;
using Notism.Shared.Exceptions;
using Notism.Shared.Models;

namespace Notism.Application.Auth.ResetPassword;

public class ResetPasswordUseCase : IRequestHandler<ResetPasswordRequest, Result<ResetPasswordResponse>>
{
    private readonly IRepository<PasswordResetToken> _passwordResetTokenRepository;
    private readonly IUserRepository _userRepository;
    private readonly IPasswordService _passwordService;

    public ResetPasswordUseCase(
        IRepository<PasswordResetToken> passwordResetTokenRepository,
        IUserRepository userRepository,
        IPasswordService passwordService)
    {
        _passwordResetTokenRepository = passwordResetTokenRepository;
        _userRepository = userRepository;
        _passwordService = passwordService;
    }

    public async Task<Result<ResetPasswordResponse>> Handle(
        ResetPasswordRequest request,
        CancellationToken cancellationToken)
    {
        var tokenSpec = new PasswordResetTokenByTokenSpecification(request.Token);
        var resetToken = await _passwordResetTokenRepository.FindByExpressionAsync(tokenSpec) ?? throw new ResultFailureException("Invalid or expired reset token");

        if (!resetToken.IsValid())
        {
            throw new ResultFailureException("Invalid or expired reset token");
        }

        var user = await _userRepository.FindByExpressionAsync(new UserByIdSpecification(resetToken.UserId)) ?? throw new ResultFailureException("User not found");

        var hashedPassword = _passwordService.HashPassword(request.NewPassword);
        var updatedUser = user.ResetPassword(hashedPassword);

        resetToken.MarkAsUsed();

        _userRepository.Update(updatedUser);
        await _passwordResetTokenRepository.SaveChangesAsync();

        return Result<ResetPasswordResponse>.Success(new ResetPasswordResponse
        {
            Message = "Password has been successfully reset.",
        });
    }
}