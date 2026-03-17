using MediatR;

using Microsoft.Extensions.Logging;

using Notism.Domain.Common.Specifications;
using Notism.Domain.User;
using Notism.Domain.User.Enums;
using Notism.Shared.Exceptions;

namespace Notism.Application.User.AdminDeleteUser;

public class AdminDeleteUserHandler : IRequestHandler<AdminDeleteUserRequest>
{
    private readonly IUserRepository _userRepository;
    private readonly ILogger<AdminDeleteUserHandler> _logger;

    public AdminDeleteUserHandler(
        IUserRepository userRepository,
        ILogger<AdminDeleteUserHandler> logger)
    {
        _userRepository = userRepository;
        _logger = logger;
    }

    public async Task Handle(AdminDeleteUserRequest request, CancellationToken cancellationToken)
    {
        if (request.TargetUserId == request.CallerUserId)
        {
            throw new ResultFailureException("You cannot delete your own account.");
        }

        var specification = new FilterSpecification<Domain.User.User>(u => u.Id == request.TargetUserId);
        var user = await _userRepository.FindByExpressionAsync(specification)
            ?? throw new NotFoundException("User not found.");

        if (user.Role == UserRole.Admin)
        {
            throw new ResultFailureException("Cannot delete another administrator.");
        }

        _userRepository.Remove(user);
        await _userRepository.SaveChangesAsync();

        _logger.LogInformation("Admin {CallerUserId} deleted user {TargetUserId}", request.CallerUserId, request.TargetUserId);
    }
}