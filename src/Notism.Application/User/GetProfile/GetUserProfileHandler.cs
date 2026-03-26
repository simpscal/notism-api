using MediatR;

using Microsoft.Extensions.Logging;

using Notism.Application.Common.Constants;
using Notism.Application.Common.Interfaces;
using Notism.Application.Common.Services;
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
    private readonly IMessages _messages;

    public GetUserProfileHandler(
        IRepository<Domain.User.User> userRepository,
        IStorageService storageService,
        ILogger<GetUserProfileHandler> logger,
        IMessages messages)
    {
        _userRepository = userRepository;
        _storageService = storageService;
        _logger = logger;
        _messages = messages;
    }

    public async Task<GetUserProfileResponse> Handle(
        GetUserProfileRequest request,
        CancellationToken cancellationToken)
    {
        var specification = new FilterSpecification<Domain.User.User>(u => u.Id == request.UserId);
        var user = await _userRepository.FindByExpressionAsync(specification)
            ?? throw new ResultFailureException(_messages.UserNotFound);

        string avatarUrl = user.AvatarUrl ?? string.Empty;
        if (!string.IsNullOrWhiteSpace(user.AvatarUrl) && !avatarUrl.IsValidUrl())
        {
            avatarUrl = _storageService.GetPublicUrl(user.AvatarUrl, StorageTypeConstants.Avatar);
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