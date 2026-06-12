using MediatR;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using Notism.Application.Common.Persistence;
using Notism.Application.Common.Services;
using Notism.Domain.User.Enums;
using Notism.Domain.User.Repositories;
using Notism.Shared.Exceptions;

using DomainUser = Notism.Domain.User.User;

namespace Notism.Application.User.AdminDeleteUser;

public class AdminDeleteUserHandler : IRequestHandler<AdminDeleteUserRequest>
{
    private readonly IUserRepository _userRepository;
    private readonly IReadDbContext _readDbContext;
    private readonly ILogger<AdminDeleteUserHandler> _logger;
    private readonly IMessages _messages;

    public AdminDeleteUserHandler(
        IUserRepository userRepository,
        IReadDbContext readDbContext,
        ILogger<AdminDeleteUserHandler> logger,
        IMessages messages)
    {
        _userRepository = userRepository;
        _readDbContext = readDbContext;
        _logger = logger;
        _messages = messages;
    }

    public async Task Handle(AdminDeleteUserRequest request, CancellationToken cancellationToken)
    {
        if (request.TargetUserId == request.CallerUserId)
        {
            throw new ResultFailureException(_messages.CannotDeleteOwnAccount);
        }

        var user = await _readDbContext.Set<DomainUser>(tracking: true)
                .Where(u => u.Id == request.TargetUserId)
                .FirstOrDefaultAsync(cancellationToken)
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