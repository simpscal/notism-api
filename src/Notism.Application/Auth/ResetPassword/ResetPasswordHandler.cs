using MediatR;

using Microsoft.EntityFrameworkCore;

using Notism.Application.Common.Persistence;
using Notism.Application.Common.Services;
using Notism.Domain.Common.Persistence;
using Notism.Domain.User;
using Notism.Shared.Exceptions;

using DomainUser = Notism.Domain.User.User;

namespace Notism.Application.Auth.ResetPassword;

public class ResetPasswordHandler : IRequestHandler<ResetPasswordRequest, ResetPasswordResponse>
{
    private readonly IReadDbContext _readDbContext;
    private readonly IPasswordService _passwordService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMessages _messages;

    public ResetPasswordHandler(
        IReadDbContext readDbContext,
        IPasswordService passwordService,
        IUnitOfWork unitOfWork,
        IMessages messages)
    {
        _readDbContext = readDbContext;
        _passwordService = passwordService;
        _unitOfWork = unitOfWork;
        _messages = messages;
    }

    public async Task<ResetPasswordResponse> Handle(
        ResetPasswordRequest request,
        CancellationToken cancellationToken)
    {
        var resetToken = await _readDbContext.Set<PasswordResetToken>(tracking: true)
                .Where(t => t.Token == request.Token && !t.IsUsed && t.ExpiresAt > DateTime.UtcNow)
                .FirstOrDefaultAsync(cancellationToken)
            ?? throw new ResultFailureException(_messages.InvalidOrExpiredResetToken);

        if (!resetToken.IsValid())
        {
            throw new ResultFailureException(_messages.InvalidOrExpiredResetToken);
        }

        await _unitOfWork.ExecuteInTransactionAsync(async () =>
        {
            var user = await _readDbContext.Set<DomainUser>(tracking: true)
                    .Where(u => u.Id == resetToken.UserId)
                    .FirstOrDefaultAsync(cancellationToken)
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