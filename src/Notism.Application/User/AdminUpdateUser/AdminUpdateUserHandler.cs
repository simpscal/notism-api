using MediatR;

using Microsoft.Extensions.Logging;

using Notism.Application.Common.Services;
using Notism.Application.User.AdminGetUserDetail;
using Notism.Domain.Common.Specifications;
using Notism.Domain.User;
using Notism.Domain.User.Enums;
using Notism.Shared.Exceptions;
using Notism.Shared.Extensions;

namespace Notism.Application.User.AdminUpdateUser;

public class AdminUpdateUserHandler : IRequestHandler<AdminUpdateUserRequest, AdminUserDetailResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly ILogger<AdminUpdateUserHandler> _logger;
    private readonly IMessages _messages;

    public AdminUpdateUserHandler(
        IUserRepository userRepository,
        ILogger<AdminUpdateUserHandler> logger,
        IMessages messages)
    {
        _userRepository = userRepository;
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

        var specification = new FilterSpecification<Domain.User.User>(u => u.Id == request.TargetUserId);
        var user = await _userRepository.FindByExpressionAsync(specification)
            ?? throw new NotFoundException(_messages.UserNotFound);

        if (Enum.TryParse<UserRole>(request.Role, true, out var newRole))
        {
            user.SetRole(newRole);
        }

        _userRepository.Update(user);
        await _userRepository.SaveChangesAsync();

        _logger.LogInformation("Admin updated user {TargetUserId}", request.TargetUserId);

        return AdminGetUserDetailHandler.MapToResponse(user);
    }
}