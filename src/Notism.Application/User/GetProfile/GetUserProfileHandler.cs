using MediatR;

using Microsoft.Extensions.Logging;

using Notism.Application.Common.Constants;
using Notism.Application.Common.Persistence;
using Notism.Application.Common.Services;
using Notism.Shared.Exceptions;
using Notism.Shared.Extensions;

namespace Notism.Application.User.GetProfile;

public class GetUserProfileHandler : IRequestHandler<GetUserProfileRequest, GetUserProfileResponse>
{
    private readonly IReadDbContext _readDbContext;
    private readonly IStorageService _storageService;
    private readonly ILogger<GetUserProfileHandler> _logger;
    private readonly IMessages _messages;

    public GetUserProfileHandler(
        IReadDbContext readDbContext,
        IStorageService storageService,
        ILogger<GetUserProfileHandler> logger,
        IMessages messages)
    {
        _readDbContext = readDbContext;
        _storageService = storageService;
        _logger = logger;
        _messages = messages;
    }

    public async Task<GetUserProfileResponse> Handle(
        GetUserProfileRequest request,
        CancellationToken cancellationToken)
    {
        var user = await new GetUserByIdQuery(_readDbContext).ExecuteAsync(request.UserId, cancellationToken)
            ?? throw new ResultFailureException(_messages.UserNotFound);

        string avatarUrl = user.AvatarUrl ?? string.Empty;
        if (!string.IsNullOrWhiteSpace(user.AvatarUrl) && !avatarUrl.IsValidUrl())
        {
            avatarUrl = _storageService.GetPublicUrl(user.AvatarUrl, StorageTypeConstants.Avatar);
        }

        _logger.LogInformation("User profile retrieved successfully for user {UserId}", request.UserId);

        return GetUserProfileResponse.FromDomain(user, avatarUrl);
    }
}