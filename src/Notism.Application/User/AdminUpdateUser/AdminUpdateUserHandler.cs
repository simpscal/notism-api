using MediatR;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using Notism.Application.Common.Persistence;
using Notism.Application.Common.Services;
using Notism.Application.User.AdminGetUserDetail;
using Notism.Domain.User.Enums;
using Notism.Domain.User.Repositories;
using Notism.Shared.Exceptions;

using DomainUser = Notism.Domain.User.User;

namespace Notism.Application.User.AdminUpdateUser;

public class AdminUpdateUserHandler : IRequestHandler<AdminUpdateUserRequest, AdminUserDetailResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly IReadDbContext _readDbContext;
    private readonly ILogger<AdminUpdateUserHandler> _logger;
    private readonly IMessages _messages;

    public AdminUpdateUserHandler(
        IUserRepository userRepository,
        IReadDbContext readDbContext,
        ILogger<AdminUpdateUserHandler> logger,
        IMessages messages)
    {
        _userRepository = userRepository;
        _readDbContext = readDbContext;
        _logger = logger;
        _messages = messages;
    }

    public async Task<AdminUserDetailResponse> Handle(
        AdminUpdateUserRequest request,
        CancellationToken cancellationToken)
    {
        if (request.TargetUserId == request.CallerUserId)
        {
            throw new ResultFailureException(_messages.CannotUpdateOwnRole);
        }

        var user = await _readDbContext.Set<DomainUser>(tracking: true)
                .Where(u => u.Id == request.TargetUserId)
                .FirstOrDefaultAsync(cancellationToken)
            ?? throw new NotFoundException(_messages.UserNotFound);

        if (Enum.TryParse<UserRole>(request.Role, true, out var newRole))
        {
            user.SetRole(newRole);
        }

        _userRepository.Update(user);
        await _userRepository.SaveChangesAsync();

        _logger.LogInformation("Admin updated user {TargetUserId}", request.TargetUserId);

        return AdminUserDetailResponse.FromDomain(user);
    }
}