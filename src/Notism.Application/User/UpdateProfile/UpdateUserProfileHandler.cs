using MediatR;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using Notism.Application.Common.Persistence;
using Notism.Application.Common.Services;
using Notism.Domain.User.Repositories;
using Notism.Shared.Exceptions;

using DomainUser = Notism.Domain.User.User;

namespace Notism.Application.User.UpdateProfile;

public class UpdateUserProfileHandler : IRequestHandler<UpdateUserProfileRequest, UpdateUserProfileResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly IReadDbContext _readDbContext;
    private readonly ILogger<UpdateUserProfileHandler> _logger;
    private readonly IMessages _messages;

    public UpdateUserProfileHandler(
        IUserRepository userRepository,
        IReadDbContext readDbContext,
        ILogger<UpdateUserProfileHandler> logger,
        IMessages messages)
    {
        _userRepository = userRepository;
        _readDbContext = readDbContext;
        _logger = logger;
        _messages = messages;
    }

    public async Task<UpdateUserProfileResponse> Handle(
        UpdateUserProfileRequest request,
        CancellationToken cancellationToken)
    {
        var user = await _readDbContext.Set<DomainUser>(tracking: true)
                .Where(u => u.Id == request.UserId)
                .FirstOrDefaultAsync(cancellationToken)
            ?? throw new ResultFailureException(_messages.UserNotFound);

        user.UpdateProfile(
            request.FirstName,
            request.LastName,
            request.AvatarUrl,
            request.Location);

        await _userRepository.SaveChangesAsync();

        _logger.LogInformation("User profile updated successfully for user {UserId}", request.UserId);

        return UpdateUserProfileResponse.FromDomain(user, "User profile updated successfully");
    }
}