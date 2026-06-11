using MediatR;

using Microsoft.Extensions.Logging;

using Notism.Application.Common.Persistence;
using Notism.Application.Common.Services;
using Notism.Shared.Exceptions;

namespace Notism.Application.User.AdminGetUserDetail;

public class AdminGetUserDetailHandler : IRequestHandler<AdminGetUserDetailRequest, AdminUserDetailResponse>
{
    private readonly IReadDbContext _readDbContext;
    private readonly ILogger<AdminGetUserDetailHandler> _logger;
    private readonly IMessages _messages;

    public AdminGetUserDetailHandler(
        IReadDbContext readDbContext,
        ILogger<AdminGetUserDetailHandler> logger,
        IMessages messages)
    {
        _readDbContext = readDbContext;
        _logger = logger;
        _messages = messages;
    }

    public async Task<AdminUserDetailResponse> Handle(
        AdminGetUserDetailRequest request,
        CancellationToken cancellationToken)
    {
        var user = await new GetUserByIdQuery(_readDbContext).ExecuteAsync(request.UserId, cancellationToken)
            ?? throw new NotFoundException(_messages.UserNotFound);

        _logger.LogInformation("Admin retrieved detail for user {UserId}", request.UserId);

        return AdminUserDetailResponse.FromDomain(user);
    }
}