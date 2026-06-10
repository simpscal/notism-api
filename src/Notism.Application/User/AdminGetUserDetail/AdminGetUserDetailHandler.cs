using MediatR;

using Microsoft.Extensions.Logging;

using Notism.Application.Common.Services;
using Notism.Domain.Common.Specifications;
using Notism.Domain.User;
using Notism.Domain.User.Repositories;
using Notism.Shared.Exceptions;

namespace Notism.Application.User.AdminGetUserDetail;

public class AdminGetUserDetailHandler : IRequestHandler<AdminGetUserDetailRequest, AdminUserDetailResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly ILogger<AdminGetUserDetailHandler> _logger;
    private readonly IMessages _messages;

    public AdminGetUserDetailHandler(
        IUserRepository userRepository,
        ILogger<AdminGetUserDetailHandler> logger,
        IMessages messages)
    {
        _userRepository = userRepository;
        _logger = logger;
        _messages = messages;
    }

    public async Task<AdminUserDetailResponse> Handle(
        AdminGetUserDetailRequest request,
        CancellationToken cancellationToken)
    {
        var specification = new FilterSpecification<Domain.User.User>(u => u.Id == request.UserId);
        var user = await _userRepository.FindByExpressionAsync(specification)
            ?? throw new NotFoundException(_messages.UserNotFound);

        _logger.LogInformation("Admin retrieved detail for user {UserId}", request.UserId);

        return AdminUserDetailResponse.FromDomain(user);
    }
}
