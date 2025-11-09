using MediatR;

using Microsoft.Extensions.Logging;

using Notism.Application.Common.Utilities;
using Notism.Domain.Common.Interfaces;
using Notism.Domain.User.Enums;
using Notism.Domain.User.Specifications;
using Notism.Shared.Exceptions;

namespace Notism.Application.User.UpdateProfile;

public class UpdateUserProfileHandler : IRequestHandler<UpdateUserProfileRequest, UpdateUserProfileResponse>
{
    private readonly IRepository<Domain.User.User> _userRepository;
    private readonly ILogger<UpdateUserProfileHandler> _logger;

    public UpdateUserProfileHandler(
        IRepository<Domain.User.User> userRepository,
        ILogger<UpdateUserProfileHandler> logger)
    {
        _userRepository = userRepository;
        _logger = logger;
    }

    public async Task<UpdateUserProfileResponse> Handle(
        UpdateUserProfileRequest request,
        CancellationToken cancellationToken)
    {
        var userToUpdate = await _userRepository.FindByExpressionAsync(
            new UserByIdSpecification(request.UserId))
        ?? throw new ResultFailureException("User not found");

        UserRole? role = EnumConverter.FromString<UserRole>(request.Role);

        var updatedUser = userToUpdate.UpdateProfile(
            request.FirstName,
            request.LastName,
            request.Email,
            role);

        await _userRepository.SaveChangesAsync();

        _logger.LogInformation("User profile updated successfully for user {UserId}", request.UserId);

        return new UpdateUserProfileResponse
        {
            UserId = updatedUser.Id,
            FirstName = updatedUser.FirstName,
            LastName = updatedUser.LastName,
            Email = updatedUser.Email?.Value ?? string.Empty,
            Role = EnumConverter.ToCamelCase(updatedUser.Role),
            Message = "User profile updated successfully",
        };
    }
}