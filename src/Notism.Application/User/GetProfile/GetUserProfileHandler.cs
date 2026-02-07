using MediatR;

using Microsoft.Extensions.Logging;

using Notism.Application.Common.Interfaces;
using Notism.Domain.Common.Interfaces;
using Notism.Domain.Common.Specifications;
using Notism.Shared.Exceptions;
using Notism.Shared.Extensions;

namespace Notism.Application.User.GetProfile;

public class GetUserProfileHandler : IRequestHandler<GetUserProfileRequest, GetUserProfileResponse>
{
    private readonly IRepository<Domain.User.User> _userRepository;
    private readonly IStorageService _storageService;
    private readonly ILogger<GetUserProfileHandler> _logger;

    public GetUserProfileHandler(
        IRepository<Domain.User.User> userRepository,
        IStorageService storageService,
        ILogger<GetUserProfileHandler> logger)
    {
        _userRepository = userRepository;
        _storageService = storageService;
        _logger = logger;
    }

    public async Task<GetUserProfileResponse> Handle(
        GetUserProfileRequest request,
        CancellationToken cancellationToken)
    {
        var specification = new FilterSpecification<Domain.User.User>(u => u.Id == request.UserId);
        var user = await _userRepository.FindByExpressionAsync(specification)
            ?? throw new ResultFailureException("User not found");

        string avatarUrl = user.AvatarUrl ?? string.Empty;
        if (!string.IsNullOrWhiteSpace(user.AvatarUrl) && !avatarUrl.IsValidUrl())
        {
            avatarUrl = _storageService.GetPublicUrl(user.AvatarUrl);
        }

        _logger.LogInformation("User profile retrieved successfully for user {UserId}", request.UserId);

        return new GetUserProfileResponse
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            Role = user.Role.ToCamelCase(),
            AvatarUrl = avatarUrl,
        };
    }
}