using MediatR;

using Microsoft.Extensions.Logging;

using Notism.Application.Common.Services;
using Notism.Domain.Common.Specifications;
using Notism.Domain.User;
using Notism.Domain.User.Enums;
using Notism.Shared.Exceptions;

namespace Notism.Application.User.AdminDeleteUser;

public class AdminDeleteUserHandler : IRequestHandler<AdminDeleteUserRequest>
{
    private readonly IUserRepository _userRepository;
    private readonly ILogger<AdminDeleteUserHandler> _logger;
    private readonly IMessages _messages;

    public AdminDeleteUserHandler(
        IUserRepository userRepository,
        ILogger<AdminDeleteUserHandler> logger,
        IMessages messages)
    {
        _userRepository = userRepository;
        _logger = logger;
        _messages = messages;
    }

    public async Task Handle(AdminDeleteUserRequest request, CancellationToken cancellationToken)
    {
        if (request.TargetUserId == request.CallerUserId)
        {
            throw new ResultFailureException(_messages.CannotDeleteOwnAccount);
        }

        var specification = new FilterSpecification<Domain.User.User>(u => u.Id == request.TargetUserId);
        var user = await _userRepository.FindByExpressionAsync(specification)
            ?? throw new NotFoundException(_messages.UserNotFound);

        if (user.Role == UserRole.Admin)
        {
            throw new ResultFailureException(_messages.CannotDeleteAdmin);
        }

        _userRepository.Remove(user);
        await _userRepository.SaveChangesAsync();

        _logger.LogInformation("Admin {CallerUserId} deleted user {TargetUserId}", request.CallerUserId, request.TargetUserId);
    }
}