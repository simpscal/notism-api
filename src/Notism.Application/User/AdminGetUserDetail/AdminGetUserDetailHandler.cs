using MediatR;

using Microsoft.Extensions.Logging;

using Notism.Domain.Common.Specifications;
using Notism.Domain.User;
using Notism.Shared.Exceptions;
using Notism.Shared.Extensions;

namespace Notism.Application.User.AdminGetUserDetail;

public class AdminGetUserDetailHandler : IRequestHandler<AdminGetUserDetailRequest, AdminUserDetailResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly ILogger<AdminGetUserDetailHandler> _logger;

    public AdminGetUserDetailHandler(
        IUserRepository userRepository,
        ILogger<AdminGetUserDetailHandler> logger)
    {
        _userRepository = userRepository;
        _logger = logger;
    }

    public async Task<AdminUserDetailResponse> Handle(
        AdminGetUserDetailRequest request,
        CancellationToken cancellationToken)
    {
        var specification = new FilterSpecification<Domain.User.User>(u => u.Id == request.UserId);
        var user = await _userRepository.FindByExpressionAsync(specification)
            ?? throw new NotFoundException("User not found.");

        _logger.LogInformation("Admin retrieved detail for user {UserId}", request.UserId);

        return MapToResponse(user);
    }

    internal static AdminUserDetailResponse MapToResponse(Domain.User.User user)
    {
        return new AdminUserDetailResponse
        {
            Id = user.Id,
            FirstName = user.FirstName ?? string.Empty,
            LastName = user.LastName ?? string.Empty,
            Email = user.Email.Value,
            Role = user.Role.ToCamelCase(),
            PhoneNumber = null,
            Location = null,
            CreatedAt = user.CreatedAt,
        };
    }
}