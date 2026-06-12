using MediatR;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using Notism.Application.Common.Persistence;
using Notism.Application.Common.Services;
using Notism.Domain.Common.Repositories;
using Notism.Shared.Exceptions;

namespace Notism.Application.Food.AdminUpdateCustomisationGroup;

public class AdminUpdateCustomisationGroupHandler : IRequestHandler<AdminUpdateCustomisationGroupRequest, AdminUpdateCustomisationGroupResponse>
{
    private readonly IRepository<Domain.Food.Food> _foodRepository;
    private readonly IReadDbContext _readDbContext;
    private readonly ILogger<AdminUpdateCustomisationGroupHandler> _logger;
    private readonly IMessages _messages;

    public AdminUpdateCustomisationGroupHandler(
        IRepository<Domain.Food.Food> foodRepository,
        IReadDbContext readDbContext,
        ILogger<AdminUpdateCustomisationGroupHandler> logger,
        IMessages messages)
    {
        _foodRepository = foodRepository;
        _readDbContext = readDbContext;
        _logger = logger;
        _messages = messages;
    }

    public async Task<AdminUpdateCustomisationGroupResponse> Handle(
        AdminUpdateCustomisationGroupRequest request,
        CancellationToken cancellationToken)
    {
        var food = await _readDbContext.Set<Domain.Food.Food>(tracking: true)
            .Where(f => f.Id == request.FoodId && !f.IsDeleted)
            .Include("CustomisationGroups.Options")
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new NotFoundException(_messages.FoodNotFound);

        var group = food.CustomisationGroups.FirstOrDefault(g => g.Id == request.GroupId)
            ?? throw new NotFoundException(_messages.CustomisationGroupNotFound);

        group.Update(request.Label, request.IsRequired, request.DisplayOrder);
        await _foodRepository.SaveChangesAsync();

        _logger.LogInformation(
            "Updated customisation group {GroupId} on food {FoodId}",
            group.Id,
            food.Id);

        return AdminUpdateCustomisationGroupResponse.FromDomain(group);
    }
}