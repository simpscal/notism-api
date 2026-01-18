using MediatR;

using Microsoft.Extensions.Logging;

using Notism.Domain.User;
using Notism.Domain.User.Specifications;
using Notism.Shared.Exceptions;
using Notism.Shared.Extensions;

namespace Notism.Application.User.UpdateProfile;

public class UpdateUserProfileHandler : IRequestHandler<UpdateUserProfileRequest, UpdateUserProfileResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly ILogger<UpdateUserProfileHandler> _logger;

    public UpdateUserProfileHandler(
        IUserRepository userRepository,
        ILogger<UpdateUserProfileHandler> logger)
    {
        _userRepository = userRepository;
        _logger = logger;
    }

    public async Task<UpdateUserProfileResponse> Handle(
        UpdateUserProfileRequest request,
        CancellationToken cancellationToken)
    {
        var user = await _userRepository.FindByExpressionAsync(
            new UserByIdSpecification(request.UserId))
        ?? throw new ResultFailureException("User not found");

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