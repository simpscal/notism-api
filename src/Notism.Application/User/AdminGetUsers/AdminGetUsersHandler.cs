using MediatR;

using Microsoft.Extensions.Logging;

using Notism.Domain.User;
using Notism.Shared.Extensions;

namespace Notism.Application.User.AdminGetUsers;

public class AdminGetUsersHandler : IRequestHandler<AdminGetUsersRequest, AdminGetUsersResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly ILogger<AdminGetUsersHandler> _logger;

    public AdminGetUsersHandler(
        IUserRepository userRepository,
        ILogger<AdminGetUsersHandler> logger)
    {
        _userRepository = userRepository;
        _logger = logger;
    }

    public async Task<AdminGetUsersResponse> Handle(
        AdminGetUsersRequest request,
        CancellationToken cancellationToken)
    {
        var specification = new AdminGetUsersSpecification(
            request.Keyword,
            request.SortBy,
            request.SortOrder);

        var pagedResult = await _userRepository.FilterPagedByExpressionAsync(specification, request);
        var items = pagedResult.Items.Select(MapToAdminGetUsersItemResponse).ToList();

        _logger.LogInformation("Retrieved {Count} users for admin portal", items.Count);

        return new AdminGetUsersResponse
        {
            TotalCount = pagedResult.TotalCount,
            Items = items,
        };
    }

    private static AdminGetUsersItemResponse MapToAdminGetUsersItemResponse(Domain.User.User user)
    {
        return new AdminGetUsersItemResponse
        {
            Id = user.Id,
            FirstName = user.FirstName ?? string.Empty,
            LastName = user.LastName ?? string.Empty,
            Email = user.Email.Value,
            Role = user.Role.ToCamelCase(),
            PhoneNumber = null,
            Location = null,
            AuthType = "email",
            CreatedAt = user.CreatedAt,
        };
    }
}