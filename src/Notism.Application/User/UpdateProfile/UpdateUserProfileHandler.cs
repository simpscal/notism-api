using MediatR;

using Microsoft.Extensions.Logging;

using Notism.Application.Common.Services;
using Notism.Domain.Common.Specifications;
using Notism.Domain.User;
using Notism.Shared.Exceptions;
using Notism.Shared.Extensions;

namespace Notism.Application.User.UpdateProfile;

public class UpdateUserProfileHandler : IRequestHandler<UpdateUserProfileRequest, UpdateUserProfileResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly ILogger<UpdateUserProfileHandler> _logger;
    private readonly IMessages _messages;

    public UpdateUserProfileHandler(
        IUserRepository userRepository,
        ILogger<UpdateUserProfileHandler> logger,
        IMessages messages)
    {
        _userRepository = userRepository;
        _logger = logger;
        _messages = messages;
    }

    public async Task<UpdateUserProfileResponse> Handle(
        UpdateUserProfileRequest request,
        CancellationToken cancellationToken)
    {
        var specification = new FilterSpecification<Domain.User.User>(u => u.Id == request.UserId);
        var user = await _userRepository.FindByExpressionAsync(specification)
            ?? throw new ResultFailureException(_messages.UserNotFound);

        user.UpdateProfile(
            request.FirstName,
            request.LastName,
            request.AvatarUrl);

        await _userRepository.SaveChangesAsync();

        _logger.LogInformation("User profile updated successfully for user {UserId}", request.UserId);

        return new UpdateUserProfileResponse
        {
            UserId = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email?.Value ?? string.Empty,
            Role = user.Role.ToCamelCase(),
            AvatarUrl = user.AvatarUrl,
            Message = "User profile updated successfully",
        };
    }
}