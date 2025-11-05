using MediatR;

using Microsoft.Extensions.Logging;

using Notism.Application.Common.Utilities;
using Notism.Domain.Common.Interfaces;
using Notism.Domain.User.Specifications;
using Notism.Shared.Exceptions;
using Notism.Shared.Models;

namespace Notism.Application.User.GetProfile;

public class GetUserProfileHandler : IRequestHandler<GetUserProfileRequest, Result<GetUserProfileResponse>>
{
    private readonly IRepository<Domain.User.User> _userRepository;
    private readonly ILogger<GetUserProfileHandler> _logger;

    public GetUserProfileHandler(
        IRepository<Domain.User.User> userRepository,
        ILogger<GetUserProfileHandler> logger)
    {
        _userRepository = userRepository;
        _logger = logger;
    }

    public async Task<Result<GetUserProfileResponse>> Handle(
        GetUserProfileRequest request,
        CancellationToken cancellationToken)
    {
        var user = await _userRepository.FindByExpressionAsync(
            new UserByIdSpecification(request.UserId))
        ?? throw new ResultFailureException("User not found");

        _logger.LogInformation("User profile retrieved successfully for user {UserId}", request.UserId);

        return Result<GetUserProfileResponse>.Success(new GetUserProfileResponse
        {
            UserId = user.Id,
            FirstName = user.FirstName,
            Email = user.Email?.Value ?? string.Empty,
            Role = EnumConverter.ToCamelCase(user.Role),
        });
    }
}
